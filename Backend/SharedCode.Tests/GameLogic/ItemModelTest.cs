using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {

	[TestFixture]
	public class ItemModelTest {
		private TestModel tm;
		private PlayerModel player;
		private SharedGameConfig config;
		private IslandTypeId island;
		private MergeBoardModel board;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;
			config = tm.GameConfig;
			island = IslandTypeId.MainIsland;
			board = player.Islands[island].MergeBoard;
			tm.PrimaryBoard = board;

			tm.BoardDeleteAllItems();
		}
	}
}
