using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerSendItemToIslandTest {
		private TestModel tm;
		private PlayerModel player;

		private IslandTypeId mainIsland;
		private MergeBoardModel mainIslandBoard;
		private IslandTypeId logIsland;
		private MergeBoardModel logIslandBoard;
		private IslandTypeId stoneIsland;
		private IslandTypeId energyIsland;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;

			mainIsland = IslandTypeId.MainIsland;
			mainIslandBoard = player.Islands[mainIsland].MergeBoard;
			logIsland = IslandTypeId.FromString("LogIsland");

			tm.BoardDeleteAllItems(mainIslandBoard);
			mainIslandBoard.CreateItem(3, 0, tm.CreateItem("BronzeBuilder:2"));
			mainIslandBoard.CreateItem(4, 0, tm.CreateItem("BronzeBuilder:3"));
			mainIslandBoard.CreateItem(5, 4, tm.CreateItem("BronzeBuilder:3", ItemState.Hidden));
			mainIslandBoard.CreateItem(6, 4, tm.CreateItem("BronzeBuilder:3", ItemState.Hidden));
			mainIslandBoard.CreateItem(5, 3, tm.CreateItem("BronzeBuilder:3", ItemState.Hidden));
			mainIslandBoard.CreateItem(6, 3, tm.CreateItem("BronzeBuilder:3", ItemState.Hidden));
			mainIslandBoard.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);

			player.Wallet.IslandTokens.Earned = 100; // Give island tokens for unlocking islands.
			tm.AssertAction(new PlayerRevealIsland(IslandTypeId.EnergyIsland));
			tm.AssertAction(new PlayerRevealIsland(logIsland));
			tm.AssertAction(new PlayerUnlockIsland(logIsland));
			logIslandBoard = player.Islands[logIsland].MergeBoard;

			tm.BoardDeleteAllItems(logIslandBoard);
			logIslandBoard.CreateItem(0, 1, tm.CreateItem("BronzeBuilder:2"));
			logIslandBoard.CreateItem(1, 1, tm.CreateItem("BronzeBuilder:3"));

			stoneIsland = IslandTypeId.FromString("StoneIsland");

			// Fill the item holder tiles to simplify asserting sent items
			mainIslandBoard.CreateItem(0, 0, tm.CreateItem("Orange:1"));
			mainIslandBoard.CreateItem(1, 0, tm.CreateItem("Orange:1"));
			logIslandBoard.CreateItem(1, 3, tm.CreateItem("Orange:1"));
			logIslandBoard.CreateItem(2, 3, tm.CreateItem("Orange:1"));

			energyIsland = IslandTypeId.EnergyIsland;
		}

		[Test]
		public void SendToIslandSuccess() {
			// BronzeBuilder:3, MainIsland -> LogIsland
			tm.AssertAction(new PlayerSendItemToIsland(mainIsland, 4, 0, logIsland));
			tm.AssertIsEmptyTile(4, 0, mainIslandBoard);
			Assert.AreEqual(1, logIslandBoard.ItemHolder.Count);
			tm.AssertItem("BronzeBuilder:3", logIslandBoard.ItemHolder[0]);

			// BronzeBuilder:3, LogIsland -> MainIsland
			tm.AssertAction(new PlayerSendItemToIsland(logIsland, 1, 1, mainIsland));
			tm.AssertIsEmptyTile(1, 1, logIslandBoard);
			Assert.AreEqual(1, mainIslandBoard.ItemHolder.Count);
			tm.AssertItem("BronzeBuilder:3", mainIslandBoard.ItemHolder[0]);
		}

		[Test]
		public void SendToIslandFailure() {
			// Item not transferable
			tm.AssertAction(new PlayerSendItemToIsland(mainIsland, 3, 0, logIsland), ActionResult.InvalidParam);
			// No such island
			IslandTypeId nonexistent = IslandTypeId.FromString("nonexistent");
			tm.AssertAction(new PlayerSendItemToIsland(mainIsland, 4, 0, nonexistent), ActionResult.InvalidParam);
			// Island not open
			tm.AssertAction(new PlayerSendItemToIsland(mainIsland, 4, 0, stoneIsland), ActionResult.InvalidState);
			// Items cannot be sent to EnergyIsland
			tm.AssertAction(new PlayerSendItemToIsland(mainIsland, 4, 0, energyIsland), ActionResult.InvalidParam);
			// No item
			tm.AssertAction(new PlayerSendItemToIsland(mainIsland, 1, 2, logIsland), ActionResult.InvalidCoordinates);
			// Item FreeForMerge
			tm.AssertAction(new PlayerSendItemToIsland(mainIsland, 5, 4, logIsland), ActionResult.InvalidCoordinates);
			// Item Hidden
			tm.AssertAction(new PlayerSendItemToIsland(mainIsland, 6, 4, logIsland), ActionResult.InvalidCoordinates);
		}
	}
}
