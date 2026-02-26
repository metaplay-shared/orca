using System;
using System.Collections.Generic;
using System.Linq;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Activables;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class EventModelVisibilityTest {
		private TestModel tm;
		private PlayerModel player;
		private SharedGameConfig config;

		private void InitModels(DateTime time) {
			tm = CommonUtils.CreateTestModel(MetaTime.FromDateTime(time));
			player = tm.PlayerModel;
			config = tm.GameConfig;
			player.PrivateProfile.FeaturesEnabled.Add(FeatureTypeId.DiscountEvents);
			player.PrivateProfile.FeaturesEnabled.Add(FeatureTypeId.ActivityEvents);
		}

		// See unit testing config for the events with DisplayName "EventModelVisibilityTest"
		[Test]
		public void EventVisibilityAndStatuses() {
			InitModels(new DateTime(2000, 1, 6, 7, 55, 0));
			player.Tick(NullChecksumEvaluator.Context);
			AssertEvents(player);
			int tickIncrement = 1000;

			tm.TickProgress(new DateTime(2000, 1, 6, 8, 0, 1), tickIncrement);
			AssertEvents(player, "EnergyBoostEveryDay:PREVIEW");
			tm.TickProgress(new DateTime(2000, 1, 6, 8, 30, 1), tickIncrement);
			AssertEvents(player, "EnergyBoostEveryDay:PREVIEW", "MergeEveryDay:PREVIEW");
			tm.TickProgress(new DateTime(2000, 1, 6, 9, 0, 1), tickIncrement);
			AssertEvents(player, "EnergyBoostEveryDay:ACTIVE", "MergeEveryDay:PREVIEW");
			tm.TickProgress(new DateTime(2000, 1, 6, 9, 30, 1), tickIncrement);
			AssertEvents(player, "EnergyBoostEveryDay:ACTIVE", "MergeEveryDay:ACTIVE");
			tm.TickProgress(new DateTime(2000, 1, 6, 9, 31, 1), tickIncrement);
			AssertEvents(player, "EnergyBoostEveryDay:ACTIVE", "MergeEveryDay:ACTIVE");
			tm.TickProgress(new DateTime(2000, 1, 6, 9, 59, 1), tickIncrement);
			AssertEvents(player, "EnergyBoostEveryDay:ACTIVE", "MergeEveryDay:ACTIVE");

			tm.TickProgress(new DateTime(2000, 1, 6, 10, 0, 1), tickIncrement);
			AssertEvents(
				player,
				"EnergyBoostEveryDay:ACTIVE",
				"MergeEveryDay:ACTIVE",
				"EnergyBoostEvery2ndDay:PREVIEW"
			);
			tm.TickProgress(new DateTime(2000, 1, 6, 10, 30, 1), tickIncrement);
			AssertEvents(
				player,
				"EnergyBoostEveryDay:ACTIVE",
				"MergeEveryDay:ACTIVE",
				"EnergyBoostEvery2ndDay:PREVIEW",
				"MergeEvery2ndDay:PREVIEW"
			);
			tm.TickProgress(new DateTime(2000, 1, 6, 12, 29, 1), tickIncrement);
			AssertEvents(
				player,
				"EnergyBoostEveryDay:ACTIVE",
				"MergeEveryDay:ACTIVE",
				"EnergyBoostEvery2ndDay:ACTIVE",
				"MergeEvery2ndDay:ACTIVE"
			);
			tm.TickProgress(new DateTime(2000, 1, 6, 12, 30, 1), tickIncrement);
			AssertEvents(
				player,
				"EnergyBoostEveryDay:ENDING_SOON",
				"MergeEveryDay:ACTIVE",
				"EnergyBoostEvery2ndDay:ACTIVE",
				"MergeEvery2ndDay:ACTIVE"
			);
			tm.TickProgress(new DateTime(2000, 1, 6, 13, 0, 1), tickIncrement);
			AssertEvents(
				player,
				"EnergyBoostEveryDay:REVIEW",
				"MergeEveryDay:ENDING_SOON",
				"EnergyBoostEvery2ndDay:ACTIVE",
				"MergeEvery2ndDay:ACTIVE"
			);
			tm.TickProgress(new DateTime(2000, 1, 6, 13, 30, 1), tickIncrement);
			AssertEvents(player, "MergeEveryDay:REVIEW", "EnergyBoostEvery2ndDay:ACTIVE", "MergeEvery2ndDay:ACTIVE");
			tm.TickProgress(new DateTime(2000, 1, 6, 13, 59, 59), tickIncrement);
			AssertEvents(player, "MergeEveryDay:REVIEW", "EnergyBoostEvery2ndDay:ACTIVE", "MergeEvery2ndDay:ACTIVE");
			tm.TickProgress(new DateTime(2000, 1, 6, 14, 00, 1), tickIncrement);
			AssertEvents(player, "EnergyBoostEvery2ndDay:ACTIVE", "MergeEvery2ndDay:ACTIVE");

			tm.TickProgress(new DateTime(2000, 1, 6, 15, 30, 1), tickIncrement);
			AssertEvents(player, "MergeEvery2ndDay:REVIEW");
			tm.TickProgress(new DateTime(2000, 1, 6, 16, 0, 1), tickIncrement);
			AssertEvents(player);
		}

		[Test]
		public void ReshowAd() {
			InitModels(new DateTime(2000, 1, 5, 9, 0, 0));
			player.Tick(NullChecksumEvaluator.Context);
			int tickIncrement = 1000;

			EventId everyDayEvent = EventId.FromString("MergeEveryDay");
			EventId every2ndDayEvent = EventId.FromString("MergeEvery2ndDay");

			ActivityEventModel everyDayModel = player.ActivityEvents.SubEnsureHasState(
				config.ActivityEvents[everyDayEvent],
				player
			);
			ActivityEventModel every2ndDayModel = player.ActivityEvents.SubEnsureHasState(
				config.ActivityEvents[every2ndDayEvent],
				player
			);
			Assert.False(everyDayModel.AdSeen);
			Assert.False(every2ndDayModel.AdSeen);

			tm.AssertAction(new PlayerViewActivityEventAd(everyDayEvent));
			tm.AssertAction(new PlayerViewActivityEventAd(every2ndDayEvent));

			tm.TickProgress(new DateTime(2000, 1, 6, 15, 30, 1), tickIncrement);
			// ReshowAd == false, the time stamp should NOT have been reset.
			Assert.AreNotEqual(MetaTime.Epoch, everyDayModel.AdSeen);
			// ReshowAd == true, the time stamp should have been reset.
			Assert.False(every2ndDayModel.AdSeen);
		}

		/// <summary>
		/// Checks the visible events of a player.
		/// </summary>
		/// <param name="playerModel">player model</param>
		/// <param name="expectedEvents">0 or more event strings of form "<event ID>:<status>". See the
		/// possible statuses in <see cref="EventString"/></param>
		private void AssertEvents(PlayerModel playerModel, params string[] expectedEvents) {
			List<IEventModel> eventModels = player.VisibleEventModels();
			List<string> actualVisibleEvents = eventModels.Where(eventModel => eventModel.Status(playerModel) != null)
				.Select(eventModel => EventString(eventModel)).ToList();
			Assert.AreEqual(
				expectedEvents.Length,
				actualVisibleEvents.Count,
				"Expected {0} visible events but got {1}",
				expectedEvents.Length,
				actualVisibleEvents.Count
			);
			foreach (string expectedEventStr in expectedEvents) {
				Assert.That(actualVisibleEvents, Has.Exactly(1).EqualTo(expectedEventStr));
			}
		}

		// Returns string containing event status in form: "<event ID>:<status>"
		private string EventString(IEventModel eventModel) {
			MetaActivableVisibleStatus status = eventModel.Status(player);
			if (status == null) {
				return $"{eventModel.EventId}:INACTIVE";
			} else if (status is MetaActivableVisibleStatus.Active) {
				return $"{eventModel.EventId}:ACTIVE";
			} else if (status is MetaActivableVisibleStatus.InPreview) {
				return $"{eventModel.EventId}:PREVIEW";
			} else if (status is MetaActivableVisibleStatus.EndingSoon) {
				return $"{eventModel.EventId}:ENDING_SOON";
			} else if (status is MetaActivableVisibleStatus.InReview) {
				return $"{eventModel.EventId}:REVIEW";
			} else {
				Console.Out.WriteLine($"Unknown status for {eventModel.EventId}: {status}");
				return $"{eventModel.EventId}:UNKNOWN_STATUS";
			}
		}
	}
}
