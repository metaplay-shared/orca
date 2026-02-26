using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerSkipBuilderTimerTest {
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

			tm.AssertAction(new PlayerUseBuilder(IslandTypeId.MainIsland, 0, 3));
		}

		[Test]
		public void Failures() {
			IslandTypeId mainIsland = IslandTypeId.MainIsland;
			IslandTypeId nonexistent = IslandTypeId.FromString("nonexistent");
			IslandTypeId berryIsland = IslandTypeId.FromString("BerryIsland");

			// No such island
			tm.AssertAction(new PlayerSkipBuilderTimer(nonexistent, 1, 2), ActionResult.InvalidParam);

			// Island not open
			tm.AssertAction(new PlayerSkipBuilderTimer(berryIsland, 1, 2), ActionResult.InvalidState);

			// Out of board
			tm.AssertAction(new PlayerSkipBuilderTimer(mainIsland, -1, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerSkipBuilderTimer(mainIsland, 0, -1), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerSkipBuilderTimer(mainIsland, 7, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerSkipBuilderTimer(mainIsland, 0, 9), ActionResult.InvalidCoordinates);

			// No item
			tm.AssertAction(new PlayerSkipBuilderTimer(mainIsland, 4, 0), ActionResult.InvalidCoordinates);

			// Item not being built
			tm.AssertAction(new PlayerSkipBuilderTimer(mainIsland, 0, 2), ActionResult.InvalidState);
		}

		[Test]
		public void Success() {
			tm.AnalyticsEventRecorder.Clear();

			int gems = player.Wallet.Gems.Value;
			tm.AssertAction(new PlayerSkipBuilderTimer(IslandTypeId.MainIsland, 0, 3), ActionResult.Success);
			Assert.AreEqual(2, player.Builders.Free);
			Assert.AreEqual(IslandTypeId.None, player.Builders.Permanent[1].Island);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEconomyAction)));

			ItemModel item = mainBoard[0, 3].Item;
			Assert.AreEqual(ItemBuildState.PendingComplete, item.BuildState);
			Assert.AreEqual(0, item.BuilderId);
			Assert.AreEqual(gems - 1, player.Wallet.Gems.Value);
		}
	}
}
