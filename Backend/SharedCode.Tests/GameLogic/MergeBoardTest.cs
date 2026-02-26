using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	/*
	 * These tests use the "MainIsland" map whose layout
	 * together with the coordinate system is shown below
	 * (copied here for convenience; the actual map layout
	 *  originates from the config Google Sheet).
	 *
	 * 8 ..###..
	 * 7 .#####.
	 * 6 .######
	 * 5 .######
	 * 4 ..#####
	 * 3 ..#####
	 * 2 #######
	 * 1 ####...
	 * 0 HH#####
	 *   0123456
	 *
	 * 8 ..222..
	 * 7 .22222.
	 * 6 .222211
	 * 5 .2...11
	 * 4 .......
	 * 3 .......
	 * 2 .......
	 * 1 .......
	 * 0 .......
	 *   0123456
	*/
	[TestFixture]
	public class MergeBoardTest {
		private TestModel tm;
		private PlayerModel player;
		private SharedGameConfig config;
		private MockPlayerModelClientListener clientListener;
		private MergeBoardModel board;
		private MetaTime currentTime;
		private RandomPCG random;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;
			config = tm.GameConfig;
			clientListener = tm.ClientListener;
			board = tm.PlayerModel.Islands[IslandTypeId.MainIsland].MergeBoard;
			tm.PrimaryBoard = board;
			tm.BoardDeleteAllItems(board);
			currentTime = tm.StartTime;
			random = RandomPCG.CreateFromSeed(6234);
		}

		private void InitBoard() {
			board.CreateItem(2, 4, tm.CreateItem("StoneCreator:1", ItemState.Hidden, skipFreeForMerge: false));
			// Row 2
			board.CreateItem(0, 2, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: true));
			board.CreateItem(1, 2, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(2, 2, tm.CreateItem("TreeStump:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(3, 2, tm.CreateItem("LogHouse:2", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(4, 2, tm.CreateItem("TreeStump:1", ItemState.Hidden, skipFreeForMerge: true));
			board.CreateItem(5, 2, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(6, 2, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			// Row 3
			board.CreateItem(2, 3, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(3, 3, tm.CreateItem("LogHouse:2", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(4, 3, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(5, 3, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(6, 3, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			// Row 4
			board.CreateItem(4, 4, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(5, 4, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(6, 4, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			// Row 5
			board.CreateItem(1, 5, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(4, 5, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(5, 5, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(6, 5, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));

			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);
		}

		[Test]
		public void DiscoverFreeForMergeItemWhenOpeningLockArea() {
			IslandTypeId lockIsland = IslandTypeId.FromString("LockIsland");
			tm.AssertAction(new PlayerRevealIsland(lockIsland));
			board = player.Islands[lockIsland].MergeBoard;
			tm.PrimaryBoard = board;
			tm.BoardDeleteAllItems(board);

			ItemModel treeStump = tm.CreateItem("TreeStump:1", ItemState.Hidden);
			board.CreateItem(4, 1, treeStump);
			board.CreateItem(3, 1, tm.CreateItem("Tool:1"));
			board.InitGame(player.HandleItemDiscovery);
			Assert.AreEqual(DiscoveryState.NotDiscovered, player.Merge.ItemDiscovery.GetState(tm.CreateItemType("TreeStump:1")));

			board.LockArea.UnlockArea('1');
			tm.AssertAction(new PlayerOpenLockArea(lockIsland, '1'));
			Assert.AreEqual(DiscoveryState.Discovered, player.Merge.ItemDiscovery.GetState(tm.CreateItemType("TreeStump:1")));
		}

		[Test]
		public void HasItemsOnDock() {
			Assert.False(board.HasItemsOnDock());
			board.ItemHolder.Add(tm.CreateItem("Gold:1"));
			board.AdjustItemHolder(player.ClientListener);
			Assert.True(board.HasItemsOnDock());
			tm.AssertAction(new PlayerMoveItemOnBoard(IslandTypeId.MainIsland, 1, 0, 2, 2));
			Assert.False(board.HasItemsOnDock());
		}

		[Test]
		public void MoveResult() {
			InitBoard();

			ItemModel logHouse1 = tm.CreateItem("LogHouse:1");
			ItemModel logHouse2 = tm.CreateItem("LogHouse:2");
			ItemModel bubbledLogHouse1 = tm.CreateItem("LogHouse:1");
			bubbledLogHouse1.Bubble = true;
			board.CreateItem(4, 0, bubbledLogHouse1);

			// Out of board
			Assert.AreEqual(MergeBoardModel.MoveResultType.Invalid, board.MoveResult(config, logHouse1, -1, 1));
			// Item holder
			Assert.AreEqual(MergeBoardModel.MoveResultType.Invalid, board.MoveResult(config, logHouse1, 0, 0));
			// Sea
			Assert.AreEqual(MergeBoardModel.MoveResultType.Invalid, board.MoveResult(config, logHouse1, 0, 3));
			// Lock area
			Assert.AreEqual(MergeBoardModel.MoveResultType.Invalid, board.MoveResult(config, logHouse1, 1, 6));
			// Building
			Assert.AreEqual(MergeBoardModel.MoveResultType.Invalid, board.MoveResult(config, logHouse1, 3, 4));
			// Hidden item
			Assert.AreEqual(MergeBoardModel.MoveResultType.Invalid, board.MoveResult(config, logHouse1, 5, 2));
			// Cannot merge or push out of its way: LogHouse:1 -> LogHouse:2 (FreeForMerge)
			Assert.AreEqual(MergeBoardModel.MoveResultType.Invalid, board.MoveResult(config, logHouse1, 3, 2));
			// Bubbled item
			Assert.AreEqual(MergeBoardModel.MoveResultType.Invalid, board.MoveResult(config, logHouse1, 4, 0));

			Assert.AreEqual(MergeBoardModel.MoveResultType.Move, board.MoveResult(config, logHouse1, 0, 1));
			Assert.AreEqual(MergeBoardModel.MoveResultType.Merge, board.MoveResult(config, logHouse1, 0, 2));
			Assert.AreEqual(MergeBoardModel.MoveResultType.Merge, board.MoveResult(config, logHouse1, 1, 2));
			Assert.AreEqual(MergeBoardModel.MoveResultType.Move, board.MoveResult(config, logHouse1, 0, 1));
			Assert.AreEqual(MergeBoardModel.MoveResultType.Move, board.MoveResult(config, logHouse2, 0, 2));

			// Moving a building
			board.RemoveItem(0, 2, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(1, 2, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(2, 2, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(3, 2, EmptyPlayerModelClientListener.Instance);

			board.CreateItem(0, 1, tm.CreateItem("StoneHouseSite:1"));
		}

		[Test]
		public void CanMoveFrom() {
			InitBoard();
			board.CreateItem(6, 6, tm.CreateItem("LogHouse:1")); // Free item in lock area
			Assert.IsTrue(board.CanMoveFrom(0, 2));
			Assert.IsFalse(board.CanMoveFrom(0, 1)); // No item
			Assert.IsFalse(board.CanMoveFrom(1, 0)); // Item holder
			Assert.IsFalse(board.CanMoveFrom(1, 2)); // FreeForMerge item
			Assert.IsFalse(board.CanMoveFrom(7, 3)); // Out of board
			Assert.IsFalse(board.CanMoveFrom(-1, 3)); // Out of board
			Assert.IsFalse(board.CanMoveFrom(2, 9)); // Out of board
			Assert.IsFalse(board.CanMoveFrom(2, -1)); // Out of board
			Assert.IsFalse(board.CanMoveFrom(1, 3)); // Sea
			Assert.IsFalse(board.CanMoveFrom(6, 6)); // Lock area
		}

		[Test]
		public void MoveResultBuilding() {
			ItemModel stoneHouseSite1 = tm.CreateItem("StoneHouseSite:1");
			board.CreateItem(2, 2, stoneHouseSite1);
			board.CreateItem(6, 2, tm.CreateItem("LogHouse:1"));

			// Out of board
			Assert.AreEqual(
				MergeBoardModel.MoveResultType.Invalid,
				board.MoveResult(config, stoneHouseSite1, -1, 1)
			);
			// Item holder
			Assert.AreEqual(
				MergeBoardModel.MoveResultType.Invalid,
				board.MoveResult(config, stoneHouseSite1, 1, 0)
			);
			// Sea
			Assert.AreEqual(
				MergeBoardModel.MoveResultType.Invalid,
				board.MoveResult(config, stoneHouseSite1, 1, 2)
			);
			Assert.AreEqual(
				MergeBoardModel.MoveResultType.Invalid,
				board.MoveResult(config, stoneHouseSite1, 3, 1)
			);
			// Lock area
			Assert.AreEqual(
				MergeBoardModel.MoveResultType.Invalid,
				board.MoveResult(config, stoneHouseSite1, 4, 4)
			);
			// Another item
			Assert.AreEqual(
				MergeBoardModel.MoveResultType.Invalid,
				board.MoveResult(config, stoneHouseSite1, 5, 2)
			);

			Assert.AreEqual(MergeBoardModel.MoveResultType.Move, board.MoveResult(config, stoneHouseSite1, 3, 2));
			Assert.AreEqual(MergeBoardModel.MoveResultType.Move, board.MoveResult(config, stoneHouseSite1, 3, 3));
			Assert.AreEqual(MergeBoardModel.MoveResultType.Move, board.MoveResult(config, stoneHouseSite1, 3, 4));
		}

		[Test]
		public void FindItem() {
			InitBoard();
			// NOTE! The items are searched in the order in which they were added to the board.
			// So, check InitBoard() to understand the assertions.
			Assert.AreEqual(board[0, 2].Item, board.FindItem(item => item.Info.Type.Value == "LogHouse"));
			Assert.AreEqual(board[0, 2].Item, board.FindItem(item => item.State == ItemState.Free));
			Assert.AreEqual(
				board[0, 2].Item,
				board.FindItem(item => item.Info.Type.Value == "LogHouse" && item.Info.Level == 1)
			);

			Assert.AreEqual(
				board[3, 2].Item,
				board.FindItem(item => item.Info.Type.Value == "LogHouse" && item.Info.Level == 2)
			);
			Assert.AreEqual(board[2, 2].Item, board.FindItem(item => item.Info.Type.Value == "TreeStump"));

			// StoneCreator was added to the board first
			Assert.AreEqual(board[2, 4].Item, board.FindItem(item => item.State == ItemState.Hidden));

			Assert.Null(board.FindItem(item => item.Info.Type.Value == "Orange"));
		}

		[Test]
		public void HasItemsForTask() {
			InitBoard();
			board[1, 5].Item.State = ItemState.Free;
			board[1, 2].Item.State = ItemState.Free;
			board[2, 2].Item.State = ItemState.Free;
			board[3, 2].Item.State = ItemState.Free;
			board[4, 2].Item.State = ItemState.Free;
			board[5, 2].Item.State = ItemState.Free;
			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);

			Assert.True(board.HasItemsForTask(tm.CreateIslandTask())); // Empty task
			Assert.True(board.HasItemsForTask(tm.CreateIslandTask("1*LogHouse:1")));
			Assert.True(board.HasItemsForTask(tm.CreateIslandTask("1*LogHouse:1", "1*LogHouse:1")));
			Assert.True(board.HasItemsForTask(tm.CreateIslandTask("2*LogHouse:1")));
			Assert.True(board.HasItemsForTask(tm.CreateIslandTask("1*TreeStump:1", "1*LogHouse:1")));
			Assert.True(board.HasItemsForTask(tm.CreateIslandTask("2*TreeStump:1", "1*LogHouse:2")));
			Assert.True(
				board.HasItemsForTask(tm.CreateIslandTask("2*TreeStump:1", "1*LogHouse:2", "3*LogHouse:1"))
			);
			Assert.True(
				board.HasItemsForTask(tm.CreateIslandTask("2*LogHouse:1", "1*LogHouse:2", "0*LogHouse:2"))
			);

			Assert.False(board.HasItemsForTask(tm.CreateIslandTask("4*LogHouse:1")));
			Assert.False(board.HasItemsForTask(tm.CreateIslandTask("2*LogHouse:1", "2*LogHouse:1")));
			Assert.False(board.HasItemsForTask(tm.CreateIslandTask("2*LogHouse:1", "3*TreeStump:1")));
			Assert.False(board.HasItemsForTask(tm.CreateIslandTask("3*TreeStump:1", "2*LogHouse:1")));
		}

		[Test]
		public void RemoveItems() {
			InitBoard();
			board[1, 5].Item.State = ItemState.Free;
			board[1, 2].Item.State = ItemState.Free;
			board[2, 2].Item.State = ItemState.Free;
			board[3, 2].Item.State = ItemState.Free;
			board[4, 2].Item.State = ItemState.Free;
			board[5, 2].Item.State = ItemState.Free;
			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);

			ChainTypeId orangeType = ChainTypeId.FromString("Orange");
			ChainTypeId logHouseType = ChainTypeId.FromString("LogHouse");
			Assert.AreEqual(0, board.RemoveItems(orangeType, level: 1, count: 10, false, clientListener));

			Assert.AreEqual(0, board.RemoveItems(orangeType, level: -1, count: 10, false, clientListener));
			Assert.AreEqual(0, board.RemoveItems(logHouseType, level: 5, count: 10, false, clientListener));
			Assert.AreEqual(0, board.RemoveItems(logHouseType, level: 1, count: 0, false, clientListener));

			Assert.AreEqual(1, board.RemoveItems(logHouseType, level: 1, count: 1, false, clientListener));
			tm.AssertIsEmptyTile(0, 2);
			ReplaceRemovedItems();

			Assert.AreEqual(2, board.RemoveItems(logHouseType, level: 1, count: 2, false, clientListener));
			tm.AssertIsEmptyTile(0, 2);
			tm.AssertIsEmptyTile(1, 2);
			ReplaceRemovedItems();

			Assert.AreEqual(3, board.RemoveItems(logHouseType, level: 1, count: 3, false, clientListener));
			tm.AssertIsEmptyTile(0, 2);
			tm.AssertIsEmptyTile(1, 2);
			tm.AssertIsEmptyTile(5, 2);
			ReplaceRemovedItems();

			Assert.AreEqual(3, board.RemoveItems(logHouseType, level: 1, count: 4, false, clientListener));
			tm.AssertIsEmptyTile(0, 2);
			tm.AssertIsEmptyTile(1, 2);
			tm.AssertIsEmptyTile(5, 2);
			tm.AssertHasItem(6, 2, "LogHouse:1", ItemState.FreeForMerge);
			ReplaceRemovedItems();

			Assert.AreEqual(1, board.RemoveItems(logHouseType, level: -1, count: 4, false, clientListener));
			tm.AssertIsEmptyTile(3, 2);
			ReplaceRemovedItems();

			Assert.AreEqual(4, board.RemoveItems(logHouseType, level: 1, count: 4, true, clientListener));
			tm.AssertIsEmptyTile(0, 2);
			tm.AssertIsEmptyTile(1, 2);
			tm.AssertIsEmptyTile(2, 3);
			tm.AssertIsEmptyTile(4, 3);
			ReplaceRemovedItems();

			Assert.AreEqual(12, board.RemoveItems(logHouseType, level: 1, count: 100, true, clientListener)
			);
			tm.AssertIsEmptyTile(4, 5);
			ReplaceRemovedItems();
		}

		private void ReplaceRemovedItems() {
			foreach (var removal in clientListener.OnItemRemovedFromBoardCalls) {
				board.CreateItem(removal.X, removal.Y, removal.Item);
			}

			clientListener.OnItemRemovedFromBoardCalls.Clear();
		}

		[Test]
		public void MarkBuildingComplete() {
			board.CreateItem(2, 1, tm.CreateItem("LogHouseSite:1"));
			board.MarkBuildingComplete(config, tm.StartTime, clientListener);
			tm.AssertHasItem(2, 1, "LogHouseSite:2");
			Assert.AreEqual(1, clientListener.OnItemRemovedFromBoardCalls.Count);
			Assert.AreEqual(1, clientListener.OnItemCreatedOnBoardCalls.Count);
			Assert.AreEqual(board[2, 1].Item, clientListener.OnItemCreatedOnBoardCalls[0].Item);
		}

		[Test]
		public void CalculateItemStates() {
			InitBoard();
			Assert.AreEqual(20, board.TotalItemCount);

			tm.AssertHasItem(0, 2, "LogHouse:1", ItemState.Free);
			tm.AssertHasItem(1, 2, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(2, 2, "TreeStump:1", ItemState.FreeForMerge);
			tm.AssertHasItem(3, 2, "LogHouse:2", ItemState.FreeForMerge);
			tm.AssertHasItem(4, 2, "TreeStump:1", ItemState.PartiallyVisible);

			board.RemoveItem(0, 2, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(1, 2, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(2, 2, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(3, 2, EmptyPlayerModelClientListener.Instance);
			// Removed 4 items -> total count: 20-4=16
			Assert.AreEqual(16, board.TotalItemCount);
			Assert.AreEqual(
				15,
				board.ItemCount(null, 0, includeLockedAreas: true, item => item.State == ItemState.Hidden)
			);

			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);

			tm.AssertHasItem(2, 3, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(3, 3, "LogHouse:2", ItemState.FreeForMerge);
			tm.AssertHasItem(4, 2, "TreeStump:1", ItemState.Free);
			tm.AssertHasItem(4, 3, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(5, 2, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(5, 3, "LogHouse:1", ItemState.PartiallyVisible);
			Assert.AreEqual(
				10,
				board.ItemCount(null, 0, includeLockedAreas: true, item => item.State == ItemState.Hidden)
			);

			// Remove item diagonally neighboring StoneCreator at (2,4)
			board.RemoveItem(4, 3, EmptyPlayerModelClientListener.Instance);
			Assert.AreEqual(15, board.TotalItemCount);
			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);

			tm.AssertHasItem(2, 4, "StoneCreator:1", ItemState.PartiallyVisible);
			tm.AssertHasItem(4, 4, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(5, 3, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(5, 4, "LogHouse:1", ItemState.PartiallyVisible);
			Assert.AreEqual(
				7,
				board.ItemCount(null, 0, includeLockedAreas: true, item => item.State == ItemState.Hidden)
			);

			// Remove FreeForMerge and Free items from rows 2 and 3
			board.RemoveItem(4, 2, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(5, 2, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(2, 3, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(3, 3, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(5, 3, EmptyPlayerModelClientListener.Instance);
			Assert.AreEqual(10, board.TotalItemCount);
			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);

			// Non-disposable creator skips FreeForMerge
			tm.AssertHasItem(2, 4, "StoneCreator:1", ItemState.Free);
			tm.AssertHasItem(6, 2, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(6, 3, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(6, 4, "LogHouse:1", ItemState.PartiallyVisible);
			tm.AssertHasItem(4, 4, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(5, 4, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(4, 5, "LogHouse:1", ItemState.FreeForMerge);

			Assert.AreEqual(
				3,
				board.ItemCount(null, 0, includeLockedAreas: true, item => item.State == ItemState.Hidden)
			);

			board.RemoveItem(6, 2, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(6, 3, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(2, 4, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(4, 4, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(5, 4, EmptyPlayerModelClientListener.Instance);

			Assert.AreEqual(5, board.TotalItemCount);
			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);
			tm.AssertHasItem(1, 5, "LogHouse:1", ItemState.Hidden);
			tm.AssertHasItem(4, 5, "LogHouse:1", ItemState.FreeForMerge);
			tm.AssertHasItem(5, 5, "LogHouse:1", ItemState.Hidden);
			tm.AssertHasItem(6, 5, "LogHouse:1", ItemState.Hidden);
			tm.AssertHasItem(6, 4, "LogHouse:1", ItemState.FreeForMerge);
		}

		[Test]
		public void CalculateItemStates2By2Item() {
			board.CreateItem(1, 1, tm.CreateItem("StoneCreator:1", ItemState.Hidden, skipFreeForMerge: false));
			// Row 2
			board.CreateItem(0, 1, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(0, 2, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(2, 0, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(3, 0, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(3, 1, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(3, 2, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(3, 3, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(2, 3, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));

			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);
			tm.AssertHasItem(1, 1, "StoneCreator:1", ItemState.Hidden);

			board.RemoveItem(3, 0, EmptyPlayerModelClientListener.Instance);
			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);
			tm.AssertHasItem(1, 1, "StoneCreator:1", ItemState.PartiallyVisible);

			board.RemoveItem(3, 1, EmptyPlayerModelClientListener.Instance);
			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);
			tm.AssertHasItem(1, 1, "StoneCreator:1", ItemState.Free);

			// Reinsert removed items and reset all item states to Hidden
			board.CreateItem(3, 0, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			board.CreateItem(3, 1, tm.CreateItem("LogHouse:1", ItemState.Hidden, skipFreeForMerge: false));
			tm.BoardApplyToAllItems(item => item.State = ItemState.Hidden);

			board.RemoveItem(3, 3, EmptyPlayerModelClientListener.Instance);
			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);
			tm.AssertHasItem(1, 1, "StoneCreator:1", ItemState.PartiallyVisible);

			board.RemoveItem(2, 3, EmptyPlayerModelClientListener.Instance);
			board.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);
			tm.AssertHasItem(1, 1, "StoneCreator:1", ItemState.Free);
		}

		[Test]
		public void InitGameAndTotalCount() {
			MockItemDiscoveryHandler discoveryHandler = new MockItemDiscoveryHandler();
			// 3 (free) items on unlocked area
			ItemModel logHouse1 = tm.CreateItem("LogHouse:1");
			board.CreateItem(1, 2, logHouse1);
			ItemModel logHouse2 = tm.CreateItem("LogHouse:2");
			board.CreateItem(6, 4, logHouse2);
			ItemModel stoneCreator1 = tm.CreateItem("StoneCreator:1");
			board.CreateItem(4, 3, stoneCreator1);
			// 1 item on locked area
			ItemModel logHouse3 = tm.CreateItem("LogHouse:3");
			board.CreateItem(1, 5, logHouse3);
			// 2 hidden items (on free area)
			ItemModel logHouse4 = tm.CreateItem("LogHouse:2", ItemState.Hidden); // NOTE: Level 2 item
			board.CreateItem(3, 0, logHouse4);
			ItemModel orange1 = tm.CreateItem("Orange:1", ItemState.Hidden);
			board.CreateItem(4, 0, orange1);

			board.InitGame(discoveryHandler.HandleItemDiscovery);
			// Only free, "discoverable" items on unlocked area "are discovered"
			Assert.AreEqual(3, discoveryHandler.Items.Count);
			CollectionAssert.Contains(discoveryHandler.Items, logHouse1);
			CollectionAssert.Contains(discoveryHandler.Items, logHouse2);
			CollectionAssert.Contains(discoveryHandler.Items, stoneCreator1);
			CollectionAssert.DoesNotContain(discoveryHandler.Items, logHouse3);
			CollectionAssert.DoesNotContain(discoveryHandler.Items, logHouse4);

			// All 5 items
			Assert.AreEqual(6, board.TotalItemCount);
			Assert.AreEqual(6, board.ItemCount());
			// Exclude locked areas
			Assert.AreEqual(5, board.ItemCount(null, 0, includeLockedAreas: false));
			// Exclude locked areas and unmovable items
			Assert.AreEqual(2, board.ItemCount(null, 0, includeLockedAreas: false, item => item.CanMove));
			// Only unmovable items (logHouse4, orange1, stoneCreator1)
			Assert.AreEqual(3, board.ItemCount(null, 0, includeLockedAreas: true, item => !item.CanMove));

			// Level 1 items (logHouse1, stoneCreator1, orange1)
			Assert.AreEqual(3, board.ItemCount(null, 1));
			// Level 2 items (logHouse2, logHouse4)
			Assert.AreEqual(2, board.ItemCount(null, 2));
			// Level 2, movable items (logHouse2)
			Assert.AreEqual(1, board.ItemCount(null, 2, includeLockedAreas: true, item => item.CanMove));
			// Level 2, unmovable items (LogHouse4)
			Assert.AreEqual(1, board.ItemCount(null, 2, includeLockedAreas: true, item => !item.CanMove));
			// Only StoneCreator:1
			Assert.AreEqual(1, board.ItemCount(ChainTypeId.FromString("StoneCreator"), 1));
			// LogHouses of any level anywhere
			Assert.AreEqual(4, board.ItemCount(ChainTypeId.FromString("LogHouse")));
			// LogHouses of any level on unlocked areas
			Assert.AreEqual(3, board.ItemCount(ChainTypeId.FromString("LogHouse"), 0, includeLockedAreas: false));
			// Movable LogHouses of any level on unlocked areas
			Assert.AreEqual(
				2,
				board.ItemCount(
					ChainTypeId.FromString("LogHouse"),
					0,
					includeLockedAreas: false,
					item => item.CanMove
				)
			);
		}

		[Test]
		public void ItemSpawner() {
			for (int i = 0; i < 3; i++) {
				ItemModel item = tm.CreateItem("Orange:1");
				board.ItemHolder.Add(item);
			}

			board.AdjustItemHolder(EmptyPlayerModelClientListener.Instance);
			tm.AssertHasItem(0, 0, "Orange:1");
			tm.AssertHasItem(1, 0, "Orange:1");
			board.RemoveItem(1, 0, EmptyPlayerModelClientListener.Instance);
			board.AdjustItemHolder(EmptyPlayerModelClientListener.Instance);
			tm.AssertHasItem(0, 0, "Orange:1");
			tm.AssertHasItem(1, 0, "Orange:1");
			board.RemoveItem(1, 0, EmptyPlayerModelClientListener.Instance);
			board.AdjustItemHolder(EmptyPlayerModelClientListener.Instance);
			Assert.AreEqual(1, board.TotalItemCount);
			tm.AssertHasItem(1, 0, "Orange:1");
		}

		[Test]
		public void AutoSpawnerSingle() {
			// Place an auto spawner in position where it is free to spawn a single item.
			ItemModel orangeTree = tm.CreateItem("OrangeTree:5");
			board.CreateItem(6, 0, orangeTree);
			SimulateBoard(durationMs: 2000, 10);

			tm.AssertHasItem(6, 0, "OrangeTree:5");
			tm.AssertHasItem(5, 0, "Orange:1");
			Assert.AreEqual(2, board.TotalItemCount);
			Assert.AreEqual(3, orangeTree.Creator.ItemCount);
		}

		[Test]
		public void AutoSpawnerTwoItems() {
			// Place an auto spawner in position where it is free to spawn two items.
			ItemModel orangeTree = tm.CreateItem("OrangeTree:5");
			board.CreateItem(5, 0, orangeTree);
			SimulateBoard(durationMs: 2000, 10);

			tm.AssertHasItem(5, 0, "OrangeTree:5");
			tm.AssertHasItem(4, 0, "Orange:1");
			tm.AssertHasItem(6, 0, "Orange:1");
			Assert.AreEqual(3, board.TotalItemCount);
			Assert.AreEqual(2, orangeTree.Creator.ItemCount);
		}

		[Test]
		public void AutoSpawnerAllItems() {
			// Place an auto spawner in position where it is free to spawn two items.
			ItemModel orangeTree = tm.CreateItem("OrangeTree:5");
			board.CreateItem(5, 0, orangeTree);
			SimulateBoard(durationMs: 2000, 10);

			tm.AssertHasItem(5, 0, "OrangeTree:5");
			tm.AssertHasItem(4, 0, "Orange:1");
			tm.AssertHasItem(6, 0, "Orange:1");
			Assert.AreEqual(3, board.TotalItemCount);
			Assert.AreEqual(2, orangeTree.Creator.ItemCount);

			// Clear the spawned items to make room for two more.
			board.RemoveItem(4, 0, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(6, 0, EmptyPlayerModelClientListener.Instance);

			SimulateBoard(durationMs: 2000, 10);
			tm.AssertHasItem(5, 0, "OrangeTree:5");
			tm.AssertHasItem(4, 0, "Orange:1");
			tm.AssertHasItem(6, 0, "Orange:1");
			Assert.AreEqual(3, board.TotalItemCount);
			Assert.AreEqual(0, orangeTree.Creator.ItemCount);

			// Clear the spawned items...
			board.RemoveItem(4, 0, EmptyPlayerModelClientListener.Instance);
			board.RemoveItem(6, 0, EmptyPlayerModelClientListener.Instance);

			SimulateBoard(durationMs: 2000, 10);
			// ...and make sure that no additional items are spawned
			// (the 4 Oranges initially contained by OrangeTree have been depleted).
			tm.AssertHasItem(5, 0, "OrangeTree:5");
			Assert.AreEqual(1, board.TotalItemCount);
			Assert.AreEqual(0, orangeTree.Creator.ItemCount);
		}

		[Test]
		public void AutoSpawnerLarge() {
			ItemModel orangeTree = tm.CreateItem("LargeOrangeTree:1");
			board.CreateItem(3, 3, orangeTree);
			SimulateBoard(durationMs: 10000000, 20);

			tm.AssertHasItem(3, 3, "LargeOrangeTree:1");
			tm.AssertHasItem(3, 4, "LargeOrangeTree:1");
			tm.AssertHasItem(2, 2, "Orange:1");
			tm.AssertHasItem(3, 2, "Orange:1");
			tm.AssertHasItem(4, 2, "Orange:1");
			tm.AssertHasItem(2, 3, "Orange:1");
			tm.AssertHasItem(4, 3, "Orange:1");
			tm.AssertHasItem(2, 4, "Orange:1");
			tm.AssertHasItem(4, 4, "Orange:1");
			tm.AssertHasItem(2, 5, "Orange:1");
			tm.AssertHasItem(3, 5, "Orange:1");
			tm.AssertHasItem(4, 5, "Orange:1");
			Assert.AreEqual(11, board.TotalItemCount);
		}

		[Test]
		public void FindClosestFreeTile() {
			// Find closest free tile to (0,1)
			Assert.AreEqual(new Coordinates(0, 1), board.FindClosestFreeTile(0, 1));
			board.CreateItem(0, 1, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(1, 1), board.FindClosestFreeTile(0, 1));
			board.CreateItem(1, 1, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(0, 2), board.FindClosestFreeTile(0, 1));
			board.CreateItem(0, 2, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(1, 2), board.FindClosestFreeTile(0, 1));
			board.CreateItem(1, 2, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(2, 0), board.FindClosestFreeTile(0, 1));
			tm.BoardDeleteAllItems(board);

			// Find closest free tile to (4,5)
			board.CreateItem(4, 5, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(3, 4), board.FindClosestFreeTile(4, 5));
			board.CreateItem(3, 4, tm.CreateItem("Orange:1"));
			board.CreateItem(4, 4, tm.CreateItem("Orange:1"));
			board.CreateItem(5, 4, tm.CreateItem("Orange:1"));
			board.CreateItem(3, 5, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(2, 3), board.FindClosestFreeTile(4, 5));
			tm.BoardDeleteAllItems(board);

			// Find closest free tile to (6,8)
			Assert.AreEqual(new Coordinates(3, 5), board.FindClosestFreeTile(6, 8));
		}

		[Test]
		public void FindClosestFreeTileWithFullBoard() {
			// Fill in free tiles with oranges
			for (int i = 0; i < board.Info.BoardHeight; i++) {
				for (int j = 0; j < board.Info.BoardWidth; j++) {
					if (board[j, i].IsFree) {
						board.CreateItem(j, i, tm.CreateItem("Orange:1"));
					}
				}
			}

			// No free tile should be found starting from any point on the board
			for (int i = 0; i < board.Info.BoardHeight; i++) {
				for (int j = 0; j < board.Info.BoardWidth; j++) {
					Assert.Null(board.FindClosestFreeTile(j, i));
				}
			}

			// Remove one item. Afterwards that tile is the only free one (and
			// should thus be found when starting from any point on the board)
			board.RemoveItem(3, 1, EmptyPlayerModelClientListener.Instance);
			for (int i = 0; i < board.Info.BoardHeight; i++) {
				for (int j = 0; j < board.Info.BoardWidth; j++) {
					Assert.AreEqual(new Coordinates(3, 1), board.FindClosestFreeTile(j, i));
				}
			}
		}

		[Test]
		public void FindFreeNeighbor() {
			// Find free neighbor of (2,2)
			Assert.AreEqual(new Coordinates(1, 1), board.FindFreeNeighbor(2, 2));
			board.CreateItem(1, 1, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(1, 2), board.FindFreeNeighbor(2, 2));
			board.CreateItem(1, 2, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(2, 1), board.FindFreeNeighbor(2, 2));

			board.CreateItem(2, 1, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(2, 3), board.FindFreeNeighbor(2, 2));
			board.CreateItem(2, 3, tm.CreateItem("Orange:1"));
			board.CreateItem(3, 1, tm.CreateItem("Orange:1"));
			board.CreateItem(3, 2, tm.CreateItem("Orange:1"));
			board.CreateItem(3, 3, tm.CreateItem("Orange:1"));
			Assert.Null(board.FindFreeNeighbor(2, 2));
			tm.BoardDeleteAllItems(board);

			// Find free neighbor of (0,0)
			Assert.AreEqual(new Coordinates(0, 1), board.FindFreeNeighbor(0, 0));
			board.CreateItem(0, 1, tm.CreateItem("Orange:1"));
			Assert.AreEqual(new Coordinates(1, 1), board.FindFreeNeighbor(0, 0));
		}

		[Test]
		public void FindCoordinates() {
			for (int y = 2; y <= 4; y++) {
				for (int x = 3; x <= 5; x++) {
					board.CreateItem(x, y, tm.CreateItem("Orange:1"));
				}
			}

			board.RemoveItem(4, 3, EmptyPlayerModelClientListener.Instance);
			ItemModel orange = tm.CreateItem("Orange:1");
			board.CreateItem(4, 3, orange);
			Assert.AreEqual(new Coordinates(4, 3), board.FindCoordinates(orange));
			ItemModel orangeNotOnBoard = tm.CreateItem("Orange:1");
			Assert.Null(board.FindCoordinates(orangeNotOnBoard));
		}

		[Test]
		public void FinishBuilders() {
			ItemModel item = tm.CreateItem("Orange:1");
			board.CreateItem(4, 3, item);
			item.StartBuilding(1);

			// Builder still occupied, item should not be completed
			OrderedSet<int> occupiedBuilders = new() { 1 };
			board.FinishBuilders(config, random, occupiedBuilders, EmptyPlayerModelClientListener.Instance, _ => { });
			Assert.AreEqual(ItemBuildState.Building, item.BuildState);

			// Builder has been released, item should be in state pending complete
			board.FinishBuilders(
				config,
				random,
				new OrderedSet<int>(),
				EmptyPlayerModelClientListener.Instance,
				_ => { }
			);
			Assert.AreEqual(ItemBuildState.PendingComplete, item.BuildState);
		}

		// Test that revealing an unmovable item results in its reveal trigger being executed.
		[Test]
		public void UnmovableItemRevealed() {
			board.CreateItem(5, 2, tm.CreateItem("LogMine:1", ItemState.Hidden, true));
			board.CreateItem(4, 2, tm.CreateItem("Tool:1", ItemState.Hidden));
			board.CreateItem(4, 3, tm.CreateItem("Tool:1", ItemState.Hidden));
			board.CreateItem(4, 4, tm.CreateItem("Tool:1", ItemState.Hidden));
			board.CreateItem(5, 4, tm.CreateItem("Tool:1", ItemState.Hidden));
			board.CreateItem(6, 4, tm.CreateItem("Tool:1", ItemState.Hidden));
			board.CreateItem(3, 2, tm.CreateItem("Tool:1"));
			board.CalculateItemStates(player.HandleItemDiscovery, clientListener);
			Assert.False(player.Triggers.Executed.ContainsKey(TriggerId.FromString("OrchardHillMineRevealed")));
			tm.AssertAction(new PlayerMoveItemOnBoard(IslandTypeId.MainIsland, 3, 2, 4, 2));
			Assert.True(player.Triggers.Executed.ContainsKey(TriggerId.FromString("OrchardHillMineRevealed")));
		}

		[Test]
		public void ReplaceItems() {
			board.CreateItem(1, 6, tm.CreateItem("StoneHouse:1"));
			board.CreateItem(2, 6, tm.CreateItem("StoneHouse:2"));
			board.CreateItem(3, 6, tm.CreateItem("StoneHouse:3"));

			board.ItemHolder.Add(tm.CreateItem("StoneHouse:3"));
			board.ItemHolder.Add(tm.CreateItem("StoneHouse:2"));
			board.ItemHolder.Add(tm.CreateItem("StoneHouse:1"));

			ItemModel creator = tm.CreateItem("IslandRewards:1");
			board.CreateItem(4, 6, creator);
			creator.Creator.ItemQueue.Add(tm.CreateItemType("StoneHouse:3"));
			creator.Creator.ItemQueue.Add(tm.CreateItemType("StoneHouse:2"));
			creator.Creator.ItemQueue.Add(tm.CreateItemType("StoneHouse:1"));

			ItemModel mine = tm.CreateItem("StoneCreator:1");
			board.CreateItem(5, 6, mine);
			mine.Mine.Queue.Add(tm.CreateItemType("StoneHouse:3"));
			mine.Mine.Queue.Add(tm.CreateItemType("StoneHouse:2"));
			mine.Mine.Queue.Add(tm.CreateItemType("StoneHouse:1"));

			board.ReplaceItems(
				config,
				currentTime,
				ReplacementContextId.FromString("TestContext"),
				EmptyPlayerModelClientListener.Instance
			);
			Assert.AreEqual(tm.CreateItemType("LogHouse:2"), board[1, 6].Item.Info.ConfigKey);
			Assert.AreEqual(tm.CreateItemType("LogHouse:3"), board[2, 6].Item.Info.ConfigKey);
			Assert.False(board[3, 6].HasItem);

			Assert.AreEqual(2, board.ItemHolder.Count);
			Assert.AreEqual(tm.CreateItemType("LogHouse:3"), board.ItemHolder[0].Info.ConfigKey);
			Assert.AreEqual(tm.CreateItemType("LogHouse:2"), board.ItemHolder[1].Info.ConfigKey);

			Assert.AreEqual(2, creator.Creator.ItemQueue.Count);
			Assert.AreEqual(tm.CreateItemType("LogHouse:3"), creator.Creator.ItemQueue[0]);
			Assert.AreEqual(tm.CreateItemType("LogHouse:2"), creator.Creator.ItemQueue[1]);

			Assert.AreEqual(2, mine.Mine.Queue.Count);
			Assert.AreEqual(tm.CreateItemType("LogHouse:3"), mine.Mine.Queue[0]);
			Assert.AreEqual(tm.CreateItemType("LogHouse:2"), mine.Mine.Queue[1]);
		}

		// SimulateBoard updates the board after moving forward in time.
		private void SimulateBoard(long durationMs, int repeat) {
			for (int i = 0; i < repeat; i++) {
				board.Update(
					random,
					ProgressMs(durationMs),
					config,
					EmptyPlayerModelClientListener.Instance,
					(_, typeId) => typeId,
					_ => { }
				);
			}
		}

		// ProgressMs moves forward in time the specified amount of milliseconds and returns the (new) current time.
		private MetaTime ProgressMs(long durationMs) {
			currentTime += new MetaDuration(durationMs);
			return currentTime;
		}
	}
}
