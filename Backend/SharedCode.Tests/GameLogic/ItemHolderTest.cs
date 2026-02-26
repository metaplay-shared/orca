using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class ItemHolderTest {
		private TestModel tm;
		private PlayerModel player;

		private IslandTypeId lockIsland;
		private MergeBoardModel lockIslandBoard;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;

			lockIsland = IslandTypeId.FromString("LockIsland");
			tm.AssertAction(new PlayerRevealIsland(lockIsland));
			lockIslandBoard = player.Islands[lockIsland].MergeBoard;
		}

		[Test]
		public void DockOnTheRight() {
			// Lock Island has its dock on the right hand side of the map. Hence, items from
			// item holder will be placed on the leftmost tiles of the dock.
			ItemModel orange = tm.CreateItem("Orange:1");
			player.AddItemToHolder(lockIsland, orange);
			tm.AssertHasItem(5, 2, "Orange:1", ItemState.Free, lockIslandBoard);
		}
	}
}
