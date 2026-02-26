using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class WildcardTest {
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
			board.CreateItem(3, 0, tm.CreateItem("BronzeWildcard:3"));
			board.CreateItem(4, 0, tm.CreateItem("BronzeWildcard:3"));
			board.CreateItem(5, 0, tm.CreateItem("BronzeWildcard:4"));
			board.CreateItem(6, 0, tm.CreateItem("BronzeWildcard:4"));

			board.CreateItem(0, 1, tm.CreateItem("SilverWildcard:3"));
			board.CreateItem(1, 1, tm.CreateItem("SilverWildcard:3"));
			board.CreateItem(2, 1, tm.CreateItem("SilverWildcard:4"));
			board.CreateItem(3, 1, tm.CreateItem("SilverWildcard:4"));

			board.CreateItem(0, 2, tm.CreateItem("LogHouse:3"));
			board.CreateItem(1, 2, tm.CreateItem("LogHouse:4"));
			board.CreateItem(2, 2, tm.CreateItem("LogHouse:5"));
			board.CreateItem(3, 2, tm.CreateItem("LogHouse:6"));
			board.CreateItem(4, 2, tm.CreateItem("LogHouse:7"));
		}

		[Test]
		public void NonMerges() {
			// Max level wildcards cannot be merged.
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 6, 0));
			tm.AssertHasItem(5, 0, "BronzeWildcard:4");
			tm.AssertHasItem(6, 0, "BronzeWildcard:4");

			// Bronze wildcard can only be used on items with level <= 4.
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 2, 2));
			tm.AssertIsEmptyTile(5, 0);
			tm.AssertHasItem(2, 2, "BronzeWildcard:4");
			tm.AssertHasItem(2, 3, "LogHouse:5");

			// Wildcard fragments don't work as wildcards.
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 1, 0, 2));
			tm.AssertHasItem(0, 1, "LogHouse:3");
			tm.AssertHasItem(0, 2, "SilverWildcard:3");

			// Different types of wildcard fragments don't merge together.
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 0, 1, 1));
			tm.AssertHasItem(1, 1, "BronzeWildcard:3");
			tm.AssertHasItem(2, 0, "SilverWildcard:3");

			// Cannot use wildcard on max level item (LogHouse:7)
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 1, 4, 2));
			tm.AssertHasItem(3, 1, "LogHouse:7");
			tm.AssertHasItem(4, 2, "SilverWildcard:4");
		}

		[Test]
		public void MergingWildcardFragments() {
			// BronzeWildcard:3 + BronzeWildcard:3 -> BronzeWildcard:4
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 0, 4, 0));
			tm.AssertIsEmptyTile(3, 0);
			tm.AssertHasItem(4, 0, "BronzeWildcard:4");
		}

		[Test]
		public void WildcardMerges() {
			// Using wildcard on a wildcard fragment: BronzeWildcard:4 + SilverWildcard:3 -> SilverWildcard:4
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 1, 1));
			tm.AssertIsEmptyTile(5, 0);
			tm.AssertHasItem(1, 1, "SilverWildcard:4");

			// BronzeWildcard:4 + LogHouse:4 -> LogHouse:5
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 6, 0, 1, 2));
			tm.AssertIsEmptyTile(6, 0);
			tm.AssertHasItem(1, 2, "LogHouse:5");

			// SilverWildcard:4 + LogHouse:6 -> LogHouse:7
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 1, 3, 2));
			tm.AssertIsEmptyTile(3, 1);
			tm.AssertHasItem(3, 2, "LogHouse:7");
		}
	}
}
