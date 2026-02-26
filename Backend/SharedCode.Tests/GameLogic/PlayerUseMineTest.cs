using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerUseMineTest {
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
			mainBoard.CreateItem(0, 4, tm.CreateItem("LogHouse:7"));
		}

		[Test]
		public void Failures() {
			IslandTypeId mainIsland = IslandTypeId.MainIsland;
			IslandTypeId nonexistent = IslandTypeId.FromString("nonexistent");
			IslandTypeId berryIsland = IslandTypeId.FromString("BerryIsland");

			// No such island
			tm.AssertAction(new PlayerUseMine(nonexistent, 1, 2), ActionResult.InvalidParam);

			// Island not open
			tm.AssertAction(new PlayerUseMine(berryIsland, 1, 2), ActionResult.InvalidState);

			// Out of board
			tm.AssertAction(new PlayerUseMine(mainIsland, -1, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerUseMine(mainIsland, 0, -1), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerUseMine(mainIsland, 7, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerUseMine(mainIsland, 0, 9), ActionResult.InvalidCoordinates);

			// No item
			tm.AssertAction(new PlayerUseMine(mainIsland, 4, 0), ActionResult.InvalidCoordinates);

			// Item not a mine
			tm.AssertAction(new PlayerUseMine(mainIsland, 0, 4), ActionResult.InvalidCoordinates);
		}

		[Test]
		public void Success() {
			int energy = player.Merge.Energy.ProducedAtUpdate;
			tm.AssertAction(new PlayerUseMine(IslandTypeId.MainIsland, 0, 2), ActionResult.Success);
			Assert.AreEqual(energy - 5, player.Merge.Energy.ProducedAtUpdate);
			Assert.AreEqual(1, player.Builders.Free);
			Assert.AreEqual(IslandTypeId.MainIsland, player.Builders.Permanent[1].Island);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerBuilderUsed)));

			ItemModel item = mainBoard[0, 2].Item;
			Assert.AreEqual(MineState.Mining, item.Mine.State);
			Assert.AreEqual(1, item.Mine.BuilderId);
			Assert.AreEqual(1, item.Mine.Queue.Count);
		}
	}
}
