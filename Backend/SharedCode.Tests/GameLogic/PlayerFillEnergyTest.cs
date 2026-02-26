using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerFillEnergyTest {
		private TestModel tm;
		private PlayerModel player;
		private int maxEnergy;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;
			maxEnergy = player.GameConfig.Merge.MaxEnergy;
		}

		[Test]
		public void NotEnoughResourcesToFillEnergy() {
			player.Wallet.Gems.Earned = 2;
			player.Wallet.Gems.Purchased = 0;

			// An energy fill costs 5 gems -> too few gems
			tm.AssertAction(new PlayerFillEnergy(), ActionResult.NotEnoughResources);
			Assert.AreEqual(2, player.Wallet.Gems.Value);
			Assert.AreEqual(tm.InitialEnergy, player.Merge.Energy.ProducedAtUpdate);
			Assert.Zero(tm.ClientListener.OnResourcesModifiedCalls.Count);
			Assert.Zero(tm.AnalyticsEventRecorder.TotalCount);
		}

		[Test]
		public void FillEnergySingle() {
			player.Wallet.Gems.Earned = 10;
			player.Wallet.Gems.Purchased = 0;

			// The first energy fill costs 5 gems -> success
			tm.AssertAction(new PlayerFillEnergy(IslandTypeId.MainIsland));
			Assert.AreEqual(5, player.Wallet.Gems.Value);
			Assert.AreEqual(tm.InitialEnergy + maxEnergy, player.Merge.Energy.ProducedAtUpdate);
			// Resources are modified two times: energy is earned; gems are consumed
			Assert.AreEqual(2, tm.ClientListener.OnResourcesModifiedCalls.Count);
			Assert.AreEqual(maxEnergy, tm.ClientListener.OnResourcesModifiedCalls[0].Diff);
			Assert.AreEqual(CurrencyTypeId.Energy, tm.ClientListener.OnResourcesModifiedCalls[0].ResourceType);
			Assert.AreEqual(-5, tm.ClientListener.OnResourcesModifiedCalls[1].Diff);
			Assert.AreEqual(CurrencyTypeId.Gems, tm.ClientListener.OnResourcesModifiedCalls[1].ResourceType);

			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEconomyAction)));

			tm.ClearClientListener();
			tm.ClearAnalyticsEventRecorder();
			// The second energy fill costs 10 gems -> not enough gems for the second fill
			tm.AssertAction(new PlayerFillEnergy(), ActionResult.NotEnoughResources);
			// The resources (energy and gems) should stay the same and no events are triggered
			Assert.AreEqual(5, player.Wallet.Gems.Value);
			Assert.AreEqual(tm.InitialEnergy + maxEnergy, player.Merge.Energy.ProducedAtUpdate);
			Assert.Zero(tm.ClientListener.OnResourcesModifiedCalls.Count);
			Assert.Zero(tm.AnalyticsEventRecorder.TotalCount);
		}
	}
}
