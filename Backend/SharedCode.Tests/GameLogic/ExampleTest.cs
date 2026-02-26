using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	/// <summary>
	/// <c>ExampleTest</c> illustrates the basic structure and common conventions of a unit test.
	/// </summary>
	[TestFixture]
	public class ExampleTest {
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

		[Test]
		public void Example() {
			board.CreateItem(3, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(4, 0, tm.CreateItem("LogHouse:1"));
			//tm.PrintBoard(); // Uncomment to inspect board state

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 0, 4, 0));
			tm.AssertIsEmptyTile(3, 0);
			tm.AssertHasItem(4, 0, "LogHouse:2");
		}
	}
}
