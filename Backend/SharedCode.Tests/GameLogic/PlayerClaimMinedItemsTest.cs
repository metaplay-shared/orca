using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerClaimMinedItemsTest {
		private TestModel tm;
		private PlayerModel player;
		private MergeBoardModel mainBoard;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;

			// Initialize MainIsland
			mainBoard = tm.PlayerModel.Islands[IslandTypeId.MainIsland].MergeBoard;
			tm.BoardDeleteAllItems(mainBoard);
			mainBoard.CreateItem(0, 2, tm.CreateItem("StoneCreator:1"));
			mainBoard.CreateItem(2, 2, tm.CreateItem("StoneCreator:1"));
			mainBoard.CreateItem(0, 4, tm.CreateItem("LogHouse:7"));

			UseMine(0, 2);
		}

		[Test]
		public void Failures() {
			IslandTypeId mainIsland = IslandTypeId.MainIsland;
			IslandTypeId nonexistent = IslandTypeId.FromString("nonexistent");
			IslandTypeId berryIsland = IslandTypeId.FromString("BerryIsland");

			// No such island
			tm.AssertAction(new PlayerClaimMinedItems(nonexistent, 1, 2), ActionResult.InvalidParam);

			// Island not open
			tm.AssertAction(new PlayerClaimMinedItems(berryIsland, 1, 2), ActionResult.InvalidState);

			// Out of board
			tm.AssertAction(new PlayerClaimMinedItems(mainIsland, -1, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerClaimMinedItems(mainIsland, 0, -1), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerClaimMinedItems(mainIsland, 7, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerClaimMinedItems(mainIsland, 0, 9), ActionResult.InvalidCoordinates);

			// No item
			tm.AssertAction(new PlayerClaimMinedItems(mainIsland, 4, 0), ActionResult.InvalidCoordinates);

			// Item not a mine
			tm.AssertAction(new PlayerClaimMinedItems(mainIsland, 0, 4), ActionResult.InvalidCoordinates);
			// The mine is in a wrong state
			tm.AssertAction(new PlayerClaimMinedItems(mainIsland, 2, 2), ActionResult.InvalidState);
		}

		[Test]
		public void Success() {
			tm.AnalyticsEventRecorder.Clear();

			tm.AssertAction(new PlayerClaimMinedItems(IslandTypeId.MainIsland, 0, 2));
			Assert.AreEqual(2, player.Builders.Free);
			Assert.AreEqual(IslandTypeId.None, player.Builders.Permanent[1].Island);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerMinedItemsClaimed)));

			ItemModel item = mainBoard[0, 2].Item;
			Assert.AreEqual(MineState.Idle, item.Mine.State);
			Assert.AreEqual(0, item.Mine.BuilderId);
			Assert.AreEqual(0, item.Mine.Queue.Count);
		}

		[Test]
		public void DisposableMine() {
			// The first mining cycles
			mainBoard.CreateItem(3, 3, tm.CreateItem("DisposableMineItem:1"));
			UseMine(3, 3);
			tm.AssertAction(new PlayerClaimMinedItems(IslandTypeId.MainIsland, 3, 3));
			UseMine(3, 3);
			tm.AssertAction(new PlayerClaimMinedItems(IslandTypeId.MainIsland, 3, 3));

			// Repair the mine once
			tm.AssertAction(new PlayerRepairMine(IslandTypeId.MainIsland, 3, 3));
			tm.TickProgress(MetaDuration.FromMinutes(10));

			// The second round of mining
			UseMine(3, 3);
			tm.AssertAction(new PlayerClaimMinedItems(IslandTypeId.MainIsland, 3, 3));
			UseMine(3, 3);
			tm.AssertAction(new PlayerClaimMinedItems(IslandTypeId.MainIsland, 3, 3));

			// The mine has disappeared and was replaced by the destruction item
			ItemModel item = mainBoard[3, 3].Item;
			Assert.AreEqual(tm.CreateItemType("StoneHouse:1"), item.Info.ConfigKey);
		}

		private void UseMine(int x, int y) {
			tm.AssertAction(new PlayerUseMine(IslandTypeId.MainIsland, x, y));
			// The mining time is 15s, run for 16 to make it finish since the time is updated _after_ the call to Tick
			tm.TickProgress(MetaDuration.FromSeconds(16));
		}
	}
}
