using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerRepairMineTest {
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

			for (int i = 0; i < 2; i++) {
				tm.AssertAction(new PlayerUseMine(IslandTypeId.MainIsland, 0, 2));
				// The mining time is 15s, run for 16 to make it finish since the time is updated _after_ the call to Tick
				tm.TickProgress(MetaDuration.FromSeconds(16));
				tm.AssertAction(new PlayerClaimMinedItems(IslandTypeId.MainIsland, 0, 2));
			}
		}

		[Test]
		public void Failures() {
			IslandTypeId mainIsland = IslandTypeId.MainIsland;
			IslandTypeId nonexistent = IslandTypeId.FromString("nonexistent");
			IslandTypeId berryIsland = IslandTypeId.FromString("BerryIsland");

			// No such island
			tm.AssertAction(new PlayerRepairMine(nonexistent, 1, 2), ActionResult.InvalidParam);

			// Island not open
			tm.AssertAction(new PlayerRepairMine(berryIsland, 1, 2), ActionResult.InvalidState);

			// Out of board
			tm.AssertAction(new PlayerRepairMine(mainIsland, -1, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerRepairMine(mainIsland, 0, -1), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerRepairMine(mainIsland, 7, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerRepairMine(mainIsland, 0, 9), ActionResult.InvalidCoordinates);

			// No item
			tm.AssertAction(new PlayerRepairMine(mainIsland, 4, 0), ActionResult.InvalidCoordinates);

			// Item not a mine
			tm.AssertAction(new PlayerRepairMine(mainIsland, 0, 4), ActionResult.InvalidCoordinates);
			// The mine is in a wrong state
			tm.AssertAction(new PlayerRepairMine(mainIsland, 2, 2), ActionResult.InvalidState);
		}

		[Test]
		public void Success() {
			tm.AnalyticsEventRecorder.Clear();

			tm.AssertAction(new PlayerRepairMine(IslandTypeId.MainIsland, 0, 2), ActionResult.Success);
			Assert.AreEqual(1, player.Builders.Free);
			Assert.AreEqual(IslandTypeId.MainIsland, player.Builders.Permanent[1].Island);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerBuilderUsed)));

			ItemModel item = mainBoard[0, 2].Item;
			Assert.AreEqual(MineState.Repairing, item.Mine.State);
			Assert.AreEqual(1, item.Mine.BuilderId);
			Assert.AreEqual(0, item.Mine.Queue.Count);

			// Not really a part of this test but make sure that the repair process works correctly.
			tm.TickProgress(MetaDuration.FromMinutes(21));
			Assert.AreEqual(MineState.Idle, item.Mine.State);
			Assert.AreEqual(0, item.Mine.BuilderId);
			Assert.AreEqual(IslandTypeId.None, player.Builders.Permanent[1].Island);
		}
	}
}
