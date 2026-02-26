using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerUseBuilderTest {
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
			mainBoard.CreateItem(0, 2, tm.CreateItem("Orange:1"));
			mainBoard.CreateItem(0, 3, tm.CreateItem("LogHouse:7"));
		}

		[Test]
		public void Failures() {
			IslandTypeId mainIsland = IslandTypeId.MainIsland;
			IslandTypeId nonexistent = IslandTypeId.FromString("nonexistent");
			IslandTypeId berryIsland = IslandTypeId.FromString("BerryIsland");

			// No such island
			tm.AssertAction(new PlayerUseBuilder(nonexistent, 1, 2), ActionResult.InvalidParam);

			// Island not open
			tm.AssertAction(new PlayerUseBuilder(berryIsland, 1, 2), ActionResult.InvalidState);

			// Out of board
			tm.AssertAction(new PlayerUseBuilder(mainIsland, -1, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerUseBuilder(mainIsland, 0, -1), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerUseBuilder(mainIsland, 7, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerUseBuilder(mainIsland, 0, 9), ActionResult.InvalidCoordinates);

			// No item
			tm.AssertAction(new PlayerUseBuilder(mainIsland, 4, 0), ActionResult.InvalidCoordinates);

			// Item not buildable
			tm.AssertAction(new PlayerUseBuilder(mainIsland, 0, 2), ActionResult.InvalidState);
		}

		[Test]
		public void Success() {
			tm.AssertAction(new PlayerUseBuilder(IslandTypeId.MainIsland, 0, 3), ActionResult.Success);
			Assert.AreEqual(1, player.Builders.Free);
			Assert.AreEqual(IslandTypeId.MainIsland, player.Builders.Permanent[1].Island);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerBuilderUsed)));

			ItemModel item = mainBoard[0, 3].Item;
			Assert.AreEqual(ItemBuildState.Building, item.BuildState);
			Assert.AreEqual(1, item.BuilderId);
		}
	}
}
