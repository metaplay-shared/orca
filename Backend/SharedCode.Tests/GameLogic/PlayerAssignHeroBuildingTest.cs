using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Model;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerAssignHeroBuildingTest {
		private TestModel tm;
		private PlayerModel player;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;
			// Unlock all but last configured heroes
			player.UnlockHero();
			player.UnlockHero();
			player.UnlockHero();
			tm.ClearAnalyticsEventRecorder();
			player.Inventory.UnlockResourceItem(ChainTypeId.FromString("Orange"));
			player.Heroes.Update(player.GameConfig, player.PlayerLevel, player.Inventory.UnlockedResourceItems,
				player.CurrentTime, EmptyPlayerModelClientListener.Instance);
		}

		[Test]
		public void GeneralFailures() {
			// No such hero
			AssertAction("Unknown", "Dock", ActionResult.InvalidParam);
			// Hero not yet unlocked
			AssertAction("HeroTest1", "Dock", ActionResult.InvalidParam);
			// The only hero in the building
			AssertAction("HeroTourist", "Workshop", ActionResult.TooFewHeroesInBuilding);
		}

		[Test]
		public void InvalidTaskState() {
			player.Heroes.Update(
				tm.GameConfig,
				player.Level.Level,
				player.Inventory.UnlockedResourceItems,
				tm.StartTime,
				EmptyPlayerModelClientListener.Instance
			);
			player.EarnResources(
				CurrencyTypeId.FromString("Orange"),
				100,
				IslandTypeId.MainIsland,
				ResourceModificationContext.Empty
			);
			tm.AssertAction(new PlayerFulfillHeroTask(HeroTypeId.FromString("HeroCaptain")));
			// Hero task is in invalid state
			AssertAction("HeroCaptain", "Dock", ActionResult.InvalidState);
		}

		[Test]
		public void NotEnoughResources() {
			player.Wallet.Gems.Earned = 2;
			player.Wallet.Gems.Purchased = 0;

			AssertAction("HeroCaptain", "Dock", ActionResult.NotEnoughResources);
			Assert.AreEqual(2, player.Wallet.Gems.Value);
			Assert.Zero(tm.ClientListener.OnResourcesModifiedCalls.Count);
			Assert.Zero(tm.AnalyticsEventRecorder.TotalCount);
		}

		[Test]
		public void Success() {
			player.Wallet.Gems.Earned = 100;
			player.Wallet.Gems.Purchased = 0;

			AssertAction("HeroCaptain", "Dock", ActionResult.Success);
			Assert.AreEqual(95, player.Wallet.Gems.Value);
			Assert.AreEqual(
				ChainTypeId.FromString("Dock"),
				player.Heroes.Heroes[HeroTypeId.FromString("HeroCaptain")].Building
			);

			AssertAction("HeroCaptain", "Workshop", ActionResult.Success);
			Assert.AreEqual(85, player.Wallet.Gems.Value);
			Assert.AreEqual(
				ChainTypeId.FromString("Workshop"),
				player.Heroes.Heroes[HeroTypeId.FromString("HeroCaptain")].Building
			);
		}

		private void AssertAction(string hero, string building, MetaActionResult expected) {
			HeroTypeId heroType = HeroTypeId.FromString(hero);
			ChainTypeId buildingType = ChainTypeId.FromString(building);
			tm.AssertAction(new PlayerAssignHeroBuilding(heroType, buildingType), expected);
		}
	}
}
