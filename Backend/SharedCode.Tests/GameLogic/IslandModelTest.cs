using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class IslandModelTest {
		private TestModel tm;
		private PlayerModel player;
		private SharedGameConfig config;
		private IslandModel mainIsland;
		private IslandModel logIsland;
		private MergeBoardModel mainIslandBoard;
		private MergeBoardModel logIslandBoard;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;
			config = tm.GameConfig;
			mainIsland = player.Islands[IslandTypeId.MainIsland];
			logIsland = player.Islands[IslandTypeId.FromString("LogIsland")];
			mainIslandBoard = mainIsland.MergeBoard;

			player.Wallet.IslandTokens.Earned = 100;
			tm.AssertAction(new PlayerRevealIsland(IslandTypeId.FromString("LogIsland")));
			tm.AssertAction(new PlayerUnlockIsland(IslandTypeId.FromString("LogIsland")));
			logIslandBoard = logIsland.MergeBoard;

			tm.BoardDeleteAllItems(mainIslandBoard);
			tm.BoardDeleteAllItems(logIslandBoard);
		}

		[Test]
		public void HasItemsToCollect() {
			Assert.False(mainIsland.HasItemsToCollect(config));
			Assert.False(logIsland.HasItemsToCollect(config));

			// Main island
			mainIslandBoard.CreateItem(0, 1, tm.CreateItem("Gold:1"));
			mainIslandBoard.CreateItem(1, 1, tm.CreateItem("Gold:4"));
			Assert.False(mainIsland.HasItemsToCollect(config));
			mainIslandBoard.CreateItem(2, 1, tm.CreateItem("Gold:5"));
			Assert.True(mainIsland.HasItemsToCollect(config));

			// Log island
			logIslandBoard.CreateItem(0, 1, tm.CreateItem("Gold:1"));
			logIslandBoard.CreateItem(1, 1, tm.CreateItem("Gold:4"));
			Assert.False(logIsland.HasItemsToCollect(config));
			logIslandBoard.CreateItem(2, 1, tm.CreateItem("Gold:5"));
			Assert.True(logIsland.HasItemsToCollect(config));
		}
	}
}
