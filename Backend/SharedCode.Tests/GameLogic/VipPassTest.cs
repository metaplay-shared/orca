using System;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Math;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class VipPassTest {
		private TestModel tm;
		private PlayerModel player;
		private VipPassesModel vipPasses;
		private MockPlayerModelClientListener clientListener;

		private IslandTypeId lockIsland;
		private MergeBoardModel lockIslandBoard;
		private IslandTypeId mainIsland;
		private MergeBoardModel mainIslandBoard;

		private void InitModels(DateTime time) {
			tm = CommonUtils.CreateTestModel(MetaTime.FromDateTime(time));
			player = tm.PlayerModel;
			clientListener = tm.ClientListener;

			vipPasses = player.VipPasses;
			lockIsland = IslandTypeId.FromString("LockIsland");
			tm.AssertAction(new PlayerRevealIsland(lockIsland));
			lockIslandBoard = player.Islands[lockIsland].MergeBoard;

			mainIsland = IslandTypeId.MainIsland;
			mainIslandBoard = player.Islands[mainIsland].MergeBoard;
			mainIslandBoard.CreateItem(2, 2, tm.CreateItem("StoneHouse:8"));
			mainIslandBoard.CreateItem(3, 2, tm.CreateItem("StoneHouse:8"));
		}

		[Test]
		public void NoVipPasses() {
			InitModels(new DateTime(1990, 1, 1, 12, 0, 0));
			AssertNoVipPasses();

			tm.AssertAction(new PlayerUseBuilder(mainIsland, 2, 2));
			int builderId = mainIslandBoard[2, 2].Item.BuilderId;
			MetaDuration buildTimeLeft = player.Builders.Permanent[builderId].CompleteAt - player.CurrentTime;
			Assert.AreEqual(MetaDuration.FromMinutes(10), buildTimeLeft);

			Assert.AreEqual(100, player.Merge.Energy.ProducedAtUpdate);
			tm.TickProgress(MetaDuration.FromHours(1), 1000);
			Assert.AreEqual(100, player.Merge.Energy.ProducedAtUpdate);
		}

		[Test]
		public void SingleVipPass() {
			InitModels(new DateTime(1990, 1, 1, 12, 0, 0));
			SimulatePurchase(InAppProductId.FromString("SummerPass"));
			Assert.AreEqual(1, clientListener.OnClaimedInAppProductCalls.Count);
			OnClaimedInAppProductArgs args = clientListener.OnClaimedInAppProductCalls[0];
			Assert.AreEqual(InAppProductId.FromString("SummerPass"), args.Product.ProductId);
			Assert.AreEqual(VipPassId.FromString("SummerVipPass"), args.Product.VipPassId);
			Assert.AreEqual(1, clientListener.OnVipPassesChangedCallCount);

			Assert.True(vipPasses.HasAnyPass);
			Assert.AreEqual(MetaDuration.FromDays(30), vipPasses.PassDuration(player.CurrentTime));
			Assert.AreEqual(F64.FromDouble(0.8), vipPasses.BuilderTimerFactor());
			Assert.AreEqual(F64.FromDouble(1.1), vipPasses.EnergyProductionFactor());
			Assert.AreEqual(10, vipPasses.MaxEnergyBoost());
			Assert.True(lockIslandBoard.LockArea.IsFree(3, 1));
			Assert.True(lockIslandBoard.LockArea.IsFree(3, 2));
			Assert.False(lockIslandBoard.LockArea.IsFree(3, 3));

			// Build time is reduced: 10 min -> 8 min (20% reduction)
			tm.AssertAction(new PlayerUseBuilder(mainIsland, 2, 2));
			int builderId = mainIslandBoard[2, 2].Item.BuilderId;
			MetaDuration buildTimeLeft = player.Builders.Permanent[builderId].CompleteAt - player.CurrentTime;
			Assert.AreEqual(MetaDuration.FromMinutes(8), buildTimeLeft);

			// Max energy is boosted: 100 -> 110
			Assert.AreEqual(100, player.Merge.Energy.ProducedAtUpdate);
			tm.TickProgress(MetaDuration.FromHours(1), 1000);
			Assert.AreEqual(110, player.Merge.Energy.ProducedAtUpdate);

			// There's an extra builder
			Assert.AreEqual(3, player.Builders.Free);
			Assert.AreEqual(3, player.Builders.Total);
			Assert.AreEqual(1, player.Builders.Temporary.Count);

			// Progress until 1 hour before the expiration of the VIP pass.
			tm.TickProgress(new DateTime(1990, 1, 31, 11, 0, 0), 1000);
			Assert.AreEqual(MetaDuration.FromHours(1), vipPasses.PassDuration(player.CurrentTime));
			// Energy production is boosted: 30/h -> 33/h (10% boost)
			player.Merge.Energy.Reset(player.CurrentTime);
			// Progress 1 hour (i.e. until VIP pass expires)
			tm.TickProgress(MetaDuration.FromHours(1), 1000);
			Assert.AreEqual(33, player.Merge.Energy.ProducedAtUpdate);

			// VIP pass expired
			tm.TickProgress(MetaDuration.FromSeconds(1), 1); // Actually get the model updated.
			Assert.AreEqual(2, clientListener.OnVipPassesChangedCallCount);
			tm.AssertAction(new PlayerEnterMap(lockIsland)); // Trigger updating VIP pass lock areas
			AssertNoVipPasses();

			// For example, energy production should be back to normal: 30/h
			tm.TickProgress(MetaDuration.FromHours(1), 1000);
			Assert.AreEqual(63, player.Merge.Energy.ProducedAtUpdate);
		}

		[Test]
		public void TwoVipPasses() {
			InitModels(new DateTime(1990, 1, 1, 12, 0, 0));
			SimulatePurchase(InAppProductId.FromString("SummerPass"));
			// Progress until 6 hours before the expiration of Summer Pass
			tm.TickProgress(new DateTime(1990, 1, 31, 6, 0, 0), 1000);
			Assert.AreEqual(MetaDuration.FromHours(6), vipPasses.PassDuration(player.CurrentTime));
			SimulatePurchase(InAppProductId.FromString("SuperPass"));
			Assert.AreEqual(MetaDuration.FromDays(3), vipPasses.PassDuration(player.CurrentTime));
			Assert.AreEqual(2, clientListener.OnVipPassesChangedCallCount);

			Assert.True(vipPasses.HasAnyPass);
			Assert.AreEqual(F64.FromDouble(0.3), vipPasses.BuilderTimerFactor());
			Assert.AreEqual(F64.FromDouble(1.6), vipPasses.EnergyProductionFactor());
			Assert.AreEqual(30, vipPasses.MaxEnergyBoost());
			Assert.True(lockIslandBoard.LockArea.IsFree(3, 1));
			Assert.True(lockIslandBoard.LockArea.IsFree(3, 2));
			Assert.True(lockIslandBoard.LockArea.IsFree(3, 3));

			// Energy production is boosted: 30/h -> 48/h (60% boost)
			player.Merge.Energy.Reset(player.CurrentTime, 50);
			tm.TickProgress(MetaDuration.FromHours(1) + MetaDuration.FromMilliseconds(200), 10);
			Assert.AreEqual(98, player.Merge.Energy.ProducedAtUpdate);

			// Max energy is boosted: 100 -> 130
			tm.TickProgress(MetaDuration.FromHours(2), 1000);
			Assert.AreEqual(130, player.Merge.Energy.ProducedAtUpdate);

			// Build time is reduced: 10 min -> 3 min (70% reduction)
			tm.AssertAction(new PlayerUseBuilder(mainIsland, 2, 2));
			int builderId = mainIslandBoard[2, 2].Item.BuilderId;
			MetaDuration buildTimeLeft = player.Builders.Permanent[builderId].CompleteAt - player.CurrentTime;
			Assert.AreEqual(MetaDuration.FromMinutes(3), buildTimeLeft);

			// Expire Summer Pass
			tm.TickProgress(new DateTime(1990, 1, 31, 12, 0, 0), 1000);
			Assert.AreEqual(MetaDuration.FromHours(66), vipPasses.PassDuration(player.CurrentTime));
			tm.TickProgress(MetaDuration.FromSeconds(1));
			Assert.AreEqual(3, clientListener.OnVipPassesChangedCallCount);
			tm.AssertAction(new PlayerEnterMap(lockIsland)); // Trigger updating VIP pass lock areas

			Assert.True(vipPasses.HasAnyPass);
			Assert.AreEqual(F64.FromDouble(0.5), vipPasses.BuilderTimerFactor());
			Assert.AreEqual(F64.FromDouble(1.5), vipPasses.EnergyProductionFactor());
			Assert.AreEqual(20, vipPasses.MaxEnergyBoost());
			Assert.False(lockIslandBoard.LockArea.IsFree(3, 1));
			Assert.False(lockIslandBoard.LockArea.IsFree(3, 2));
			Assert.True(lockIslandBoard.LockArea.IsFree(3, 3));

			tm.TickProgress(MetaDuration.FromHours(68), 1000);
			tm.AssertAction(new PlayerEnterMap(lockIsland)); // Trigger updating VIP pass lock areas
			AssertNoVipPasses();
		}

		[Test]
		public void ExtendVipPass() {
			InitModels(new DateTime(1990, 1, 1, 12, 0, 0));
			SimulatePurchase(InAppProductId.FromString("SummerPass"));
			Assert.AreEqual(MetaDuration.FromDays(30), vipPasses.PassDuration(player.CurrentTime));
			tm.TickProgress(MetaDuration.FromDays(1), 1000);
			Assert.AreEqual(MetaDuration.FromDays(29), vipPasses.PassDuration(player.CurrentTime));
			SimulatePurchase(InAppProductId.FromString("SummerPass"));
			Assert.AreEqual(MetaDuration.FromDays(59), vipPasses.PassDuration(player.CurrentTime));
			Assert.AreEqual(2, clientListener.OnVipPassesChangedCallCount);
		}

		[Test]
		public void DailyReward() {
			InitModels(new DateTime(1990, 1, 1, 12, 0, 0));
			player.TimeZoneInfo.CurrentUtcOffset = MetaDuration.FromHours(0);

			Assert.AreEqual(0, player.Rewards.Count);
			SimulatePurchase(InAppProductId.FromString("SummerPass"));
			Assert.AreEqual(1, player.Rewards.Count);
			RewardModel reward = player.Rewards[0];
			Assert.AreEqual(RewardType.VipPassDaily, reward.Metadata.Type);
			Assert.AreEqual(1, reward.Resources.Count);
			ResourceInfo resourceInfo = reward.Resources[0];
			Assert.AreEqual(12, resourceInfo.Amount);
			Assert.AreEqual(CurrencyTypeId.Energy, resourceInfo.Type);

			Assert.AreEqual(2, reward.Items.Count);
			ItemCountInfo itemCountInfo0 = reward.Items[0];
			Assert.AreEqual(ChainTypeId.FromString("Tool"), itemCountInfo0.Type);
			Assert.AreEqual(2, itemCountInfo0.Level);
			Assert.AreEqual(1, itemCountInfo0.Count);
			ItemCountInfo itemCountInfo1 = reward.Items[1];
			Assert.AreEqual(ChainTypeId.FromString("Tool"), itemCountInfo1.Type);
			Assert.AreEqual(3, itemCountInfo1.Level);
			Assert.AreEqual(2, itemCountInfo1.Count);

            // No new reward given on the same day.
            CommonUtils.FastForwardTime(player, MetaDuration.FromHours(1));
			player.OnSessionStarted();
			Assert.AreEqual(1, player.Rewards.Count);

            // Only one reward given after long inactivity.
            CommonUtils.FastForwardTime(player, MetaDuration.FromDays(29));
			player.OnSessionStarted();
			Assert.AreEqual(2, player.Rewards.Count);
            // No new reward given on the same day.
            CommonUtils.FastForwardTime(player, MetaDuration.FromHours(1));
			player.OnSessionStarted();
			Assert.AreEqual(2, player.Rewards.Count);

            // Now new reward given after VIP pass expired.
            CommonUtils.FastForwardTime(player, MetaDuration.FromHours(25));
			player.OnSessionStarted();
			Assert.AreEqual(2, player.Rewards.Count);
		}

		[Test]
		public void MultipleDailyRewards() {
			InitModels(new DateTime(1990, 1, 1, 12, 0, 0));
			player.TimeZoneInfo.CurrentUtcOffset = MetaDuration.FromHours(0);

			Assert.AreEqual(0, player.Rewards.Count);
			SimulatePurchase(InAppProductId.FromString("SummerPass"));
			SimulatePurchase(InAppProductId.FromString("SuperPass"));
			Assert.AreEqual(2, player.Rewards.Count);
			RewardModel superReward = player.Rewards[1];
			Assert.AreEqual(RewardType.VipPassDaily, superReward.Metadata.Type);
			Assert.AreEqual(1, superReward.Resources.Count);
			Assert.AreEqual(0, superReward.Items.Count);

            // No new reward given on the same day.
            CommonUtils.FastForwardTime(player, MetaDuration.FromHours(1));
			player.OnSessionStarted();
			Assert.AreEqual(2, player.Rewards.Count);

            CommonUtils.FastForwardTime(player, MetaDuration.FromHours(11));
			player.OnSessionStarted();
			Assert.AreEqual(4, player.Rewards.Count);
		}

		[Test]
		public void DailyRewardBasedOnLocalTime() {
			// Local time is 17:00
			InitModels(new DateTime(1990, 1, 1, 12, 0, 0));
			player.TimeZoneInfo.CurrentUtcOffset = MetaDuration.FromHours(5);

			Assert.AreEqual(0, player.Rewards.Count);
			SimulatePurchase(InAppProductId.FromString("SummerPass"));
			Assert.AreEqual(1, player.Rewards.Count);

            // Progress from 17:00 to 23:59 -> no reward yet
            CommonUtils.FastForwardTime(player, MetaDuration.FromMinutes(6 * 60 + 59));
			player.OnSessionStarted();
			Assert.AreEqual(1, player.Rewards.Count);

            // Progress until midnight -> new reward should be given.
            CommonUtils.FastForwardTime(player, MetaDuration.FromMinutes(1));
			player.OnSessionStarted();
			Assert.AreEqual(2, player.Rewards.Count);
		}

		private void AssertNoVipPasses() {
			Assert.False(vipPasses.HasAnyPass);
			Assert.AreEqual(MetaDuration.Zero, vipPasses.PassDuration(player.CurrentTime));
			Assert.AreEqual(F64.One, vipPasses.BuilderTimerFactor());
			Assert.AreEqual(F64.One, vipPasses.EnergyProductionFactor());
			Assert.AreEqual(0, vipPasses.MaxEnergyBoost());
			Assert.False(lockIslandBoard.LockArea.IsFree(3, 1));
			Assert.False(lockIslandBoard.LockArea.IsFree(3, 2));
			Assert.False(lockIslandBoard.LockArea.IsFree(3, 3));
		}

		private void SimulatePurchase(InAppProductId productId) {
			InAppProductInfo productInfo = player.GameConfig.InAppProducts[productId];
			InAppPurchaseEvent purchaseEvent =
				InAppPurchaseEvent
					.ForDevelopment("transaction-id", productId, "platform-product-id", "receipt", "signature");
			player.OnClaimedInAppProduct(purchaseEvent, productInfo, out ResolvedPurchaseContentBase _);
		}
	}
}
