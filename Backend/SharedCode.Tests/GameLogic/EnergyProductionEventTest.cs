using System;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	// See https://docs.google.com/spreadsheets/d/1pKGzbvA3wI0AKq5zdkHbZoCEuTdrIRuMEAwNwVxmv1g/edit#gid=1216926414&range=A1
	// for the relevant configuration.
	[TestFixture]
	public class EnergyProductionEventTest {
		private TestModel tm;
		private PlayerModel player;

		private void Init(DateTime time) {
			MetaTime startTime = MetaTime.FromDateTime(time);
			tm = CommonUtils.CreateTestModel(startTime);
			player = tm.PlayerModel;
			player.PrivateProfile.FeaturesEnabled.Add(FeatureTypeId.DiscountEvents);
		}

		[Test]
		public void WeekendFiesta() {
			// Start in the middle of "WeekendFiesta" event
			Init(new DateTime(2022, 5, 21, 12, 0, 0));

			EventId eventId = EventId.FromString("WeekendFiesta");
			// Check event metadata
			DiscountEventModel eventModel = player.DiscountEvents.SubEnsureHasState(
				player.GameConfig.DiscountEvents[eventId],
				player
			);
			Assert.AreEqual("Weekend Fiesta", eventModel.Info.DisplayName);
			Assert.AreEqual("Energy production boosted by 50%", eventModel.Info.Description);
			Assert.AreEqual("Energy:1.50", eventModel.Info.DisplayShortInfo);

			Assert.False(eventModel.AdSeen);
			tm.AssertAction(new PlayerViewActivityEventAd(eventId));
			Assert.True(eventModel.AdSeen);

			player.Merge.Energy.Reset(tm.StartTime, 0);
			tm.TickProgress(MetaDuration.FromHours(1));
			player.Tick(NullChecksumEvaluator.Context); // Trigger update
			// There should be one hour of production boosted by 50% (normal rate: 30/h): 1h * 1.5*30/h = 45
			Assert.AreEqual(45, player.Merge.Energy.ProducedAtUpdate);

			tm.TickProgress(MetaDuration.FromDays(2), 100);
			Assert.False(eventModel.AdSeen);
		}

		[Test]
		public void SingleBoostHourTick() {
			// Start half an hour before the start of "SingleBoostHour" event
			Init(new DateTime(2022, 5, 18, 11, 30, 0));

			EventId eventId = EventId.FromString("SingleBoostHour");
			DiscountEventModel eventModel = player.DiscountEvents.SubEnsureHasState(
				player.GameConfig.DiscountEvents[eventId],
				player
			);

			Assert.False(eventModel.AdSeen);
			tm.AssertAction(new PlayerViewActivityEventAd(eventId));
			Assert.True(eventModel.AdSeen);

			player.Merge.Energy.Reset(tm.StartTime, 0);
			tm.TickProgress(MetaDuration.FromHours(1));

			Assert.AreEqual(1, tm.ClientListener.OnEventStateChangedCalls.Count);
			Assert.AreEqual(
				EventId.FromString("SingleBoostHour"),
				tm.ClientListener.OnEventStateChangedCalls[0].EventId
			);

			tm.TickProgress(MetaDuration.FromHours(1));
			// There should be
			// * one hour of normal production 11:30-12:00 and 13:00-13:30 -> 1h * 30/h = 30
			// * one hour of boosted production 12:00-13:00 -> 1h * 1.2*30/h = 36
			Assert.AreEqual(30 + 36, player.Merge.Energy.ProducedAtUpdate);

			// Check that the finalization event has been sent.
			Assert.AreEqual(2, tm.ClientListener.OnEventStateChangedCalls.Count);
			Assert.AreEqual(
				EventId.FromString("SingleBoostHour"),
				tm.ClientListener.OnEventStateChangedCalls[1].EventId
			);

			// AdSeen state should not have been reset.
			Assert.True(eventModel.AdSeen);
		}

		[Test]
		public void SingleBoostHourFastForward() {
			// Start half an hour before the start of "SingleBoostHour" event
			Init(new DateTime(2022, 5, 18, 11, 30, 0));

			player.Merge.Energy.Reset(tm.StartTime, 0);
			CommonUtils.FastForwardTime(player, MetaDuration.FromHours(2));

			// There should be
			// * one hour of normal production 11:30-12:00 and 13:00-13:30 -> 1h * 30/h = 30
			// * one hour of boosted production 12:00-13:00 -> 1h * 1.2*30/h = 36
			Assert.AreEqual(30 + 36, player.Merge.Energy.ProducedAtUpdate);
		}

		[Test]
		public void SingleBoostHourOnRestoredFromPersistedState() {
			// Start half an hour before the start of "SingleBoostHour" event
			Init(new DateTime(2022, 5, 18, 11, 30, 0));

			player.Merge.Energy.Reset(tm.StartTime, 0);
			player.OnRestoredFromPersistedState(
				player.CurrentTime + MetaDuration.FromHours(2),
				MetaDuration.FromHours(2)
			);

			Assert.AreEqual(30 + 36, player.Merge.Energy.ProducedAtUpdate);
		}
	}
}
