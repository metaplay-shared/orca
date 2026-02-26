using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerCreateMergeItemTest {
		private TestModel tm;
		private PlayerModel player;
		private MockPlayerModelClientListener clientListener;
		private MetaTime startTime;
		private MergeBoardModel board;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;
			clientListener = tm.ClientListener;
			startTime = tm.StartTime;
			board = player.Islands[IslandTypeId.MainIsland].MergeBoard;

			tm.BoardDeleteAllItems(board);
			board.CreateItem(0, 2, tm.CreateItem("Orange:1"));
			board.CreateItem(1, 1, tm.CreateItem("ToolBox:3"));
			board.CreateItem(2, 1, tm.CreateItem("ToolBox:4"));
			board[2, 1].Item.Creator.Producer.Reset(tm.StartTime);
			board.CreateItem(3, 1, tm.CreateItem("ToolBox:4"));
			board.CreateItem(4, 3, tm.CreateItem("TreeStump:1"));

			tm.ClientListener.OnItemCreatedOnBoardCalls.Clear();
		}

		[Test]
		public void Failures() {
			IslandTypeId mainIsland = IslandTypeId.MainIsland;
			IslandTypeId nonexistent = IslandTypeId.FromString("nonexistent");
			IslandTypeId berryIsland = IslandTypeId.FromString("BerryIsland");

			// No such island
			tm.AssertAction(new PlayerCreateMergeItem(nonexistent, 1, 2), ActionResult.InvalidParam);

			// Island not open
			tm.AssertAction(new PlayerCreateMergeItem(berryIsland, 1, 2), ActionResult.InvalidState);

			// Out of board
			tm.AssertAction(new PlayerCreateMergeItem(mainIsland, -1, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerCreateMergeItem(mainIsland, 0, -1), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerCreateMergeItem(mainIsland, 7, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerCreateMergeItem(mainIsland, 0, 9), ActionResult.InvalidCoordinates);

			// No item
			tm.AssertAction(new PlayerCreateMergeItem(mainIsland, 2, 0), ActionResult.InvalidCoordinates);

			// Toolbox:3 cannot spawn items
			tm.AssertAction(new PlayerCreateMergeItem(mainIsland, 1, 1), ActionResult.InvalidState);

			// No items left in the creator
			board[2, 1].Item.Creator.Producer.Reset(tm.StartTime);
			tm.AssertAction(new PlayerCreateMergeItem(mainIsland, 2, 1), ActionResult.NoItemsLeft);

			// No energy left
			player.Merge.Energy.Reset(tm.StartTime);
			tm.AssertAction(new PlayerCreateMergeItem(mainIsland, 3, 1), ActionResult.NotEnoughResources);

			// No place to spawn the item (not really an error)
			player.Merge.Energy.Reset(tm.StartTime, 10);
			tm.BoardFill(tm.CreateItem("Orange:1"), board, fillLockedAreas: false);
			int itemCount = board.TotalItemCount;
			tm.AssertAction(new PlayerCreateMergeItem(mainIsland, 3, 1), ActionResult.Success);
			Assert.AreEqual(itemCount, board.ItemCount()); // No item should have been spawned
		}

		[Test]
		public void Success() {
			int energy = player.Merge.Energy.ProducedAtUpdate;
			int itemCount = board.TotalItemCount;
			tm.AssertAction(new PlayerCreateMergeItem(IslandTypeId.MainIsland, 3, 1), ActionResult.Success);
			Assert.AreEqual(itemCount + 1, board.TotalItemCount);
			Assert.AreEqual(energy - 1, player.Merge.Energy.ProducedAtUpdate);

			Assert.AreEqual(1, tm.ClientListener.OnItemCreatedOnBoardCalls.Count);
			Assert.AreEqual(1, tm.ClientListener.OnItemDiscoveryChangedCalls.Count);
		}

		[Test]
		public void SuccessDisposable() {
			// Trigger item discovery for the items initially on the board.
			board.CalculateItemStates(player.HandleItemDiscovery, player.ClientListener);
			clientListener.OnItemDiscoveryChangedCalls.Clear();

			int itemCount = board.TotalItemCount;
			board[4, 3].Item.Creator.Producer.Reset(startTime, 1); // Reset TreeStump
			tm.AssertAction(new PlayerCreateMergeItem(IslandTypeId.MainIsland, 4, 3), ActionResult.Success);
			Assert.AreEqual(itemCount + 1, board.TotalItemCount);
			Assert.AreEqual(1, tm.ClientListener.OnItemRemovedFromBoardCalls.Count); // TreeStump removed

			// 2 LogHouse:1 items created (one spawned; one destruction item)
			Assert.AreEqual(2, tm.ClientListener.OnItemCreatedOnBoardCalls.Count);
			Assert.AreEqual(1, tm.ClientListener.OnItemDiscoveryChangedCalls.Count);
		}
	}
}
