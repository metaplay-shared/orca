using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerCollectMergeItemTest {
		private TestModel tm;
		private PlayerModel player;
		private MergeBoardModel mainBoard;
		private MergeBoardModel logIslandBoard;
		private IslandTypeId mainIsland;
		private IslandTypeId logIsland;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;
			mainIsland = IslandTypeId.MainIsland;
			logIsland = IslandTypeId.FromString("LogIsland");

			// Initialize MainIsland
			mainBoard = player.Islands[mainIsland].MergeBoard;
			tm.BoardDeleteAllItems(mainBoard);
			mainBoard.CreateItem(0, 1, tm.CreateItem("Gold:3"));
			mainBoard.CreateItem(0, 2, tm.CreateItem("Orange:1"));
			mainBoard.CreateItem(1, 2, tm.CreateItem("Gem:3"));
			mainBoard.CreateItem(2, 0, tm.CreateItem("HeroCook:2"));
			mainBoard.CreateItem(5, 0, tm.CreateItem("LogHouse:1"));
			mainBoard.CreateItem(6, 0, tm.CreateItem("Orange:1"));
			mainBoard.CreateItem(3, 4, tm.CreateItem("HeroCaptain:4"));

			mainBoard.CreateItem(5, 2, tm.CreateItem("Gold:4", ItemState.Hidden));
			mainBoard.CreateItem(6, 2, tm.CreateItem("Gold:4", ItemState.Hidden));
			mainBoard.CreateItem(5, 3, tm.CreateItem("Gold:4", ItemState.Hidden));
			mainBoard.CreateItem(6, 3, tm.CreateItem("Gold:4", ItemState.Hidden));

			mainBoard.CalculateItemStates(_ => { }, EmptyPlayerModelClientListener.Instance);

			// Reveal LogIsland
			tm.AssertAction(new PlayerRevealIsland(logIsland));

			// Open LogIsland
			player.Wallet.IslandTokens.Earned = 100;
			tm.AssertAction(new PlayerUnlockIsland(logIsland));
			Assert.AreEqual(90, player.Wallet.IslandTokens.Value);

			// Initialize LogIsland
			logIslandBoard = tm.PlayerModel.Islands[logIsland].MergeBoard;
			tm.BoardDeleteAllItems(logIslandBoard);
			logIslandBoard.CreateItem(0, 0, tm.CreateItem("Orange:1"));
			logIslandBoard.CreateItem(1, 0, tm.CreateItem("HeroCaptain:1"));
			logIslandBoard.CreateItem(2, 0, tm.CreateItem("IslandToken:2"));
			logIslandBoard.CreateItem(0, 2, tm.CreateItem("Gem:3"));
			logIslandBoard.CreateItem(0, 2, tm.CreateItem("LogHouse:3"));
			logIslandBoard.CreateItem(3, 1, tm.CreateItem("Gold:4"));

			// Fill the item holder tiles to simplify asserting the claimed rewards.
			mainBoard.CreateItem(0, 0, tm.CreateItem("Orange:1"));
			mainBoard.CreateItem(1, 0, tm.CreateItem("Orange:1"));
			logIslandBoard.CreateItem(1, 3, tm.CreateItem("Orange:1"));
			logIslandBoard.CreateItem(2, 3, tm.CreateItem("Orange:1"));

			tm.ClientListener.OnItemCreatedOnBoardCalls.Clear();
		}

		[Test]
		public void CollectFailures() {
			IslandTypeId nonexistent = IslandTypeId.FromString("nonexistent");
			IslandTypeId berryIsland = IslandTypeId.FromString("BerryIsland");

			// No such island
			tm.AssertAction(new PlayerCollectMergeItem(nonexistent, 1, 2), ActionResult.InvalidParam);

			// Island not open
			tm.AssertAction(new PlayerCollectMergeItem(berryIsland, 1, 2), ActionResult.InvalidState);

			// Out of board
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, -1, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 0, -1), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 7, 0), ActionResult.InvalidCoordinates);
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 0, 9), ActionResult.InvalidCoordinates);

			// No item
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 4, 0), ActionResult.InvalidCoordinates);

			// Item not collectable
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 2, 0), ActionResult.InvalidState);

			// Item under locked area
			// TODO: Fails at the moment because ItemModel.CanCollect() is used to for the check
			// but lock area information is one level higher in MergeBoardModel.
			// tm.AssertAction(new PlayerCollectMergeItem(logIsland, 3, 1), ActionResult.InvalidState);

			// Non-free items (i.e. FreeForMerge or Hidden)
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 5, 2), ActionResult.InvalidState);
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 6, 2), ActionResult.InvalidState);
		}

		[Test]
		public void CollectSuccess() {
			tm.AnalyticsEventRecorder.Clear();
			int goldEarned = player.Wallet.Gold.Earned;

			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 0, 1), ActionResult.Success); // Collect Gem:1
			Assert.AreEqual(goldEarned + 1, player.Wallet.Gold.Earned);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEconomyAction)));

			// Precondition
			int gemsEarned = player.Wallet.Gems.Earned;
			Assert.False(player.Triggers.Executed.ContainsKey(TriggerId.FromString("GemCollected")));
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 1, 2), ActionResult.Success); // Collect Gem:3
			Assert.AreEqual(gemsEarned + 1, player.Wallet.Gems.Earned);
			Assert.True(player.Triggers.Executed.ContainsKey(TriggerId.FromString("GemCollected")));
			Assert.AreEqual(2, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEconomyAction)));

			Assert.False(player.Heroes.Heroes.ContainsKey(HeroTypeId.FromString("HeroCaptain")));
			Assert.AreEqual(HeroTypeId.FromString("HeroCaptain"), player.Heroes.CurrentHero);
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 3, 4), ActionResult.Success); // Unlock hero
			// HeroCaptain:1 should have been removed after unlocking the hero
			tm.AssertIsEmptyTile(1, 0, logIslandBoard);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerHeroUnlocked)));
			Assert.True(player.Heroes.Heroes.ContainsKey(HeroTypeId.FromString("HeroCaptain")));
			Assert.AreEqual(HeroTypeId.FromString("HeroCook"), player.Heroes.CurrentHero);

			int islandTokensEarned = player.Wallet.IslandTokens.Earned;
			tm.AssertAction(new PlayerCollectMergeItem(logIsland, 2, 0), ActionResult.Success);
			// Island tokens sent to main island (should not affect balance)
			Assert.AreEqual(islandTokensEarned, player.Wallet.IslandTokens.Earned);
			Assert.AreEqual(1, mainBoard.ItemHolder.Count);
			tm.AssertItem("IslandToken:2", mainBoard.ItemHolder[0]);
			Assert.AreEqual(1, tm.ClientListener.OnItemTransferredToIslandCalls.Count);

			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 0, 2), ActionResult.Success);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerMergeItemCollected)));
			Assert.AreEqual(1, player.Inventory.Resources.Count);
			Assert.AreEqual(1, player.Inventory.Resources[CurrencyTypeId.FromString("Orange")]);
		}

		[Test]
		public void CollectBetweenIslands() {
			// LogHouse already on LogIsland
			tm.AssertAction(new PlayerCollectMergeItem(logIsland, 0, 2), ActionResult.InvalidState);
			// LogHouse: MainIsland -> LogIsland
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 5, 0), ActionResult.Success);
			tm.AssertIsEmptyTile(5, 0, mainBoard);
			Assert.AreEqual(1, logIslandBoard.ItemHolder.Count);
			tm.AssertItem("LogHouse:1", logIslandBoard.ItemHolder[0]);
			Assert.AreEqual(0, player.Inventory.Resources.Count);

			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 6, 0), ActionResult.Success); // Orange
			tm.AssertIsEmptyTile(6, 0, mainBoard);
			Assert.AreEqual(1, player.Inventory.Resources.Count);
			Assert.AreEqual(1, player.Inventory.Resources[CurrencyTypeId.FromString("Orange")]);
			tm.AssertAction(new PlayerCollectMergeItem(logIsland, 0, 0), ActionResult.Success); // Orange
			tm.AssertIsEmptyTile(0, 0, logIslandBoard);
			Assert.AreEqual(1, player.Inventory.Resources.Count);
			Assert.AreEqual(2, player.Inventory.Resources[CurrencyTypeId.FromString("Orange")]);

			tm.AssertAction(new PlayerCollectMergeItem(logIsland, 2, 0), ActionResult.Success); // IslandToken
			tm.AssertIsEmptyTile(2, 0, logIslandBoard);
			Assert.AreEqual(1, mainBoard.ItemHolder.Count);
			tm.AssertItem("IslandToken:2", mainBoard.ItemHolder[0]);
		}

		[Test]
		public void CollectItemWithActiveBuilder() {
			ItemModel logHouse = tm.CreateItem("LogHouse:6");
			logHouse.BuildState = ItemBuildState.NotStarted;
			mainBoard.CreateItem(0, 1, logHouse);
			tm.AssertAction(new PlayerUseBuilder(mainIsland, 0, 1));
			Assert.AreEqual(mainIsland, player.Builders.Permanent[logHouse.UsedBuilderId].Island);
			tm.AssertAction(new PlayerCollectMergeItem(mainIsland, 0, 1));
			Assert.AreEqual(logIsland, player.Builders.Permanent[logHouse.UsedBuilderId].Island);
		}
	}
}
