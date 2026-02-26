using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class ConsumableBuilderTest {
		private TestModel tm;
		private PlayerModel player;
		private MergeBoardModel board;
		private IslandTypeId island;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;

			island = IslandTypeId.MainIsland;
			board = player.Islands[island].MergeBoard;
			tm.PrimaryBoard = board;

			tm.BoardDeleteAllItems(board);
			board.CreateItem(3, 0, tm.CreateItem("BronzeBuilder:1"));
			board.CreateItem(4, 0, tm.CreateItem("BronzeBuilder:2"));
			board.CreateItem(5, 0, tm.CreateItem("BronzeBuilder:3"));
			board.CreateItem(6, 0, tm.CreateItem("BronzeBuilder:3"));

			board.CreateItem(0, 2, tm.CreateItem("StoneHouse:3"));
			board.CreateItem(1, 2, tm.CreateItem("StoneHouse:4"));
			board.CreateItem(2, 2, tm.CreateItem("StoneHouse:5"));
			board.CreateItem(3, 2, tm.CreateItem("StoneHouse:6"));
			board.CreateItem(4, 2, tm.CreateItem("StoneHouse:7"));
			board.CreateItem(5, 2, tm.CreateItem("StoneHouse:8"));
		}

		[Test]
		public void BuilderUsedEntirely() {
			// BronzeBuilder:3 -> StoneHouse:7 (build time: 3 min)
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 4, 2));
			tm.AssertHasItem(3, 1, "BronzeBuilder:1");

			ItemModel stoneHouse = board[4, 2].Item;
			Assert.AreEqual(ItemBuildState.Building, stoneHouse.BuildState);
			int builderId = stoneHouse.BuilderId;
			Assert.AreEqual(
				MetaDuration.FromMinutes(1),
				player.Builders.Consumable[builderId].CompleteAt - player.CurrentTime
			);
		}

		[Test]
		public void BuilderUsedPartly() {
			// BronzeBuilder:3 -> StoneHouse:6 (build time: 1 min)
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 3, 2));
			ItemModel bronzeBuilder = board[2, 1].Item;
			tm.AssertItem("BronzeBuilder:3", bronzeBuilder);
			Assert.AreEqual(MetaDuration.FromMinutes(1), bronzeBuilder.Booster.BuildTime);

			ItemModel stoneHouse = board[3, 2].Item;
			Assert.AreEqual(ItemBuildState.PendingComplete, stoneHouse.BuildState);
			Assert.AreEqual(0, stoneHouse.BuilderId);

			player.Tick(NullChecksumEvaluator.Context);
			Assert.AreEqual(0, player.Builders.Consumable.Count);
		}

		[Test]
		public void BuilderUsedEntirelyWithExistingBuilder() {
			// BronzeBuilder:3 -> StoneHouse:7 (build time: 3 min)
			tm.AssertAction(new PlayerUseBuilder(island, 4, 2));

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 4, 2));
			tm.AssertHasItem(3, 1, "BronzeBuilder:1");

			ItemModel stoneHouse = board[4, 2].Item;
			Assert.AreEqual(ItemBuildState.Building, stoneHouse.BuildState);
			int builderId = stoneHouse.BuilderId;
			Assert.AreEqual(
				MetaDuration.FromMinutes(1),
				player.Builders.Permanent[builderId].CompleteAt - player.CurrentTime
			);

			tm.TickProgress(MetaDuration.FromMilliseconds(60100));
			Assert.AreEqual(ItemBuildState.PendingComplete, stoneHouse.BuildState);

			Assert.AreEqual(0, player.Builders.Consumable.Count);
		}

		[Test]
		public void BuilderUsedPartlyWithExistingBuilder() {
			// BronzeBuilder:3 -> StoneHouse:6 (build time: 1 min)
			tm.AssertAction(new PlayerUseBuilder(island, 3, 2));

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 3, 2));
			ItemModel bronzeBuilder = board[2, 1].Item;
			tm.AssertItem("BronzeBuilder:3", bronzeBuilder);
			Assert.AreEqual(MetaDuration.FromMinutes(1), bronzeBuilder.Booster.BuildTime);

			ItemModel stoneHouse = board[3, 2].Item;
			Assert.AreEqual(ItemBuildState.PendingComplete, stoneHouse.BuildState);
			Assert.AreEqual(0, stoneHouse.BuilderId);

			Assert.AreEqual(0, player.Builders.Consumable.Count);
		}

		[Test]
		public void TwoConsumableBuilders() {
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 4, 2));
			ItemModel bronzeBuilder1 = board[3, 1].Item;
			tm.AssertItem("BronzeBuilder:1", bronzeBuilder1);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 6, 0, 5, 2));
			ItemModel bronzeBuilder2 = board[6, 2].Item;
			tm.AssertItem("BronzeBuilder:1", bronzeBuilder2);

			ItemModel stoneHouse7 = board[4, 2].Item;
			ItemModel stoneHouse8 = board[5, 2].Item;
			Assert.AreNotEqual(stoneHouse7.BuilderId, stoneHouse8.BuilderId);

			Assert.AreEqual(ItemBuildState.Building, stoneHouse7.BuildState);
			int builderId7 = stoneHouse7.BuilderId;
			Assert.AreEqual(
				MetaDuration.FromMinutes(1),
				player.Builders.Consumable[builderId7].CompleteAt - player.CurrentTime
			);

			Assert.AreEqual(ItemBuildState.Building, stoneHouse8.BuildState);
			int builderId8 = stoneHouse8.BuilderId;
			Assert.AreEqual(
				MetaDuration.FromMinutes(8),
				player.Builders.Consumable[builderId8].CompleteAt - player.CurrentTime
			);
		}

		[Test]
		public void BuilderAppliedToNonFreeItem() {
			board.CreateItem(5, 4, tm.CreateItem("StoneHouse:6", ItemState.Hidden));
			board.CreateItem(6, 4, tm.CreateItem("StoneHouse:6", ItemState.Hidden));
			board.CreateItem(5, 3, tm.CreateItem("StoneHouse:6", ItemState.Hidden));
			board.CreateItem(6, 3, tm.CreateItem("StoneHouse:6", ItemState.Hidden));
			board.CalculateItemStates(tm.PlayerModel.HandleItemDiscovery, tm.ClientListener);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 6, 4), ActionResult.InvalidCoordinates);
		}
	}
}
