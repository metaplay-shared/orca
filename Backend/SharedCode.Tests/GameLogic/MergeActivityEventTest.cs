using System;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Activables;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class MergeActivityEventTest {
		private TestModel tm;
		private PlayerModel player;
		private MockPlayerModelClientListener clientListener;
		private SharedGameConfig config;
		private MergeBoardModel board;
		private IslandTypeId island;

		private void InitModels(DateTime time) {
			tm = CommonUtils.CreateTestModel(MetaTime.FromDateTime(time));
			player = tm.PlayerModel;
			config = tm.GameConfig;
			clientListener = tm.ClientListener;

			player.PrivateProfile.FeaturesEnabled.Add(FeatureTypeId.ActivityEvents);
			island = IslandTypeId.MainIsland;
			board = player.Islands[island].MergeBoard;
			InitBoard();
		}

		private void InitBoard() {
			tm.BoardDeleteAllItems(board);
			board.CreateItem(3, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(4, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(5, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(6, 0, tm.CreateItem("LogHouse:1"));

			board.CreateItem(0, 1, tm.CreateItem("LogHouse:2"));
			board.CreateItem(1, 1, tm.CreateItem("LogHouse:2"));
			board.CreateItem(2, 1, tm.CreateItem("LogHouse:2"));
			board.CreateItem(3, 1, tm.CreateItem("LogHouse:2"));

			board.CreateItem(0, 2, tm.CreateItem("LogHouse:3"));
			board.CreateItem(1, 2, tm.CreateItem("LogHouse:3"));
			board.CreateItem(2, 2, tm.CreateItem("LogHouse:3"));
			board.CreateItem(3, 2, tm.CreateItem("LogHouse:3"));

			board.CreateItem(2, 5, tm.CreateItem("IslandToken:2"));
			board.CreateItem(3, 5, tm.CreateItem("IslandToken:2"));
			board.CreateItem(4, 5, tm.CreateItem("IslandToken:3"));
			board.CreateItem(4, 4, tm.CreateItem("IslandToken:3"));

			// Fill the item holder tiles to simplify asserting the claimed rewards.
			board.CreateItem(0, 0, tm.CreateItem("Orange:1"));
			board.CreateItem(1, 0, tm.CreateItem("Orange:1"));
		}

		[Test]
		public void ModelExistence() {
			InitModels(new DateTime(2009, 5, 16, 3, 59, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");

			// Make sure that the model is present already before the start of the first occurrence
			// of the event and even before the preview period of it.
			IEventModel model = player.TryGetEventModel(eventId);
			Assert.NotNull(model);
		}

		[Test]
		public void SingleRewardSteps() {
			// Start 1 minute before the start of "MondayMergeFest" event
			InitModels(new DateTime(2022, 5, 16, 3, 59, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");

			// Test that the event model is initialized already before the start of the first occurrence of the event.
			tm.AssertAction(new PlayerPurchaseActivityEventPremiumPass(eventId), ActionResult.InvalidState);

			Assert.True(player.ActivityEvents.IsInPreview(config.ActivityEvents[eventId], player));
			ActivityEventModel eventModel =
				player.ActivityEvents.SubEnsureHasState(config.ActivityEvents[eventId], player);
			Assert.AreEqual("Merge Collect", eventModel.Info.DisplayName);
			Assert.AreEqual("Earn rewards by merging items", eventModel.Info.Description);
			Assert.AreEqual("MondayMergeFest, type: Merge", eventModel.Info.DisplayShortInfo);
			// Merge during the preview period should not affect the event
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 5, 3, 5));

			AssertEventModel(eventId, xp: 0, totalXp: 0, level: 0);

			// Start event
			tm.TickProgress(MetaDuration.FromMinutes(2));
			Assert.AreEqual(1, tm.ClientListener.OnEventStateChangedCalls.Count);
			Assert.AreEqual(
				EventId.FromString("MondayMergeFest"),
				tm.ClientListener.OnEventStateChangedCalls[0].EventId
			);
			AssertEventModel(eventId, xp: 0, totalXp: 0, level: 0, terminated: false);
			Assert.AreEqual(MetaTime.FromDateTime(new DateTime(2022, 5, 16, 4, 0, 0)), eventModel.StartTime);

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 0, 4, 0));
			AssertEventModel(eventId, xp: 5, totalXp: 5, level: 0);
			Assert.AreEqual(1, clientListener.OnActivityEventScoreAddedCalls.Count);
			OnActivityEventScoreAddedArgs scoreAddedArgs = clientListener.OnActivityEventScoreAddedCalls[0];
			Assert.AreEqual(EventId.FromString("MondayMergeFest"), scoreAddedArgs.EventId);
			Assert.AreEqual(5, scoreAddedArgs.Delta);
			Assert.AreEqual(0, scoreAddedArgs.Level);
			Assert.True(scoreAddedArgs.Context is MergeBoardResourceContext);
			MergeBoardResourceContext context = (MergeBoardResourceContext)scoreAddedArgs.Context;
			Assert.AreEqual(4, context.X);
			Assert.AreEqual(0, context.Y);

			// No rewards gained yet
			tm.AssertAction(new PlayerClaimActivityEventRewards(eventId));
			Assert.AreEqual(0, board.ItemHolder.Count);
			Assert.AreEqual(0, clientListener.OnRewardAddedCallCount);

			// Level up: 1
			clientListener.OnActivityEventScoreAddedCalls.Clear();
			tm.AnalyticsEventRecorder.Clear();
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 6, 0));
			AssertEventModel(eventId, xp: 0, totalXp: 10, level: 1);
			Assert.AreEqual(1, clientListener.OnActivityEventScoreAddedCalls.Count);
			scoreAddedArgs = clientListener.OnActivityEventScoreAddedCalls[0];
			Assert.AreEqual(EventId.FromString("MondayMergeFest"), scoreAddedArgs.EventId);
			Assert.AreEqual(1, scoreAddedArgs.Level);
			tm.AssertAction(new PlayerClaimActivityEventRewards(eventId));
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEventRewardsClaimed)));
			PlayerItemDiscovered discoveryEvent = (PlayerItemDiscovered)tm.AnalyticsEventRecorder.Events[0];
			Assert.AreEqual(1, discoveryEvent.Level);
			Assert.AreEqual(ChainTypeId.FromString("Gold"), discoveryEvent.Type);
			PlayerEventRewardsClaimed claimEvent = (PlayerEventRewardsClaimed)tm.AnalyticsEventRecorder.Events[1];
			Assert.AreEqual("MondayMergeFest", claimEvent.EventId);
			Assert.AreEqual("Merge", claimEvent.Type);
			Assert.False(claimEvent.HasPremiumPass);
			Assert.AreEqual(0, claimEvent.RewardsClaimedBefore);
			Assert.AreEqual(1, claimEvent.RewardsClaimed);
			Assert.False(claimEvent.AutoClaim);

			Assert.AreEqual(1, board.ItemHolder.Count);
			tm.AssertItem("Gold:1", board.ItemHolder[0]);
			Assert.AreEqual(0, clientListener.OnRewardAddedCallCount);

			// Cannot claim rewards twice
			tm.AssertAction(new PlayerClaimActivityEventRewards(eventId));
			Assert.AreEqual(1, board.ItemHolder.Count);

			// Buy premium pass
			tm.AnalyticsEventRecorder.Clear();
			tm.AssertAction(new PlayerPurchaseActivityEventPremiumPass(eventId), ActionResult.NotEnoughResources);
			Assert.AreEqual(0, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEconomyAction)));
			Assert.AreEqual(0, tm.ClientListener.OnActivityEventPremiumPassBoughtCalls.Count);
			Assert.False(eventModel.HasPremiumPass());
			player.Wallet.Gems.Purchased = 150;
			tm.AssertAction(new PlayerPurchaseActivityEventPremiumPass(eventId));
			Assert.True(eventModel.HasPremiumPass());
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEconomyAction)));
			tm.AssertAction(new PlayerPurchaseActivityEventPremiumPass(eventId));
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEconomyAction)));
			Assert.AreEqual(1, tm.ClientListener.OnActivityEventPremiumPassBoughtCalls.Count);
			Assert.AreEqual(
				"MondayMergeFest",
				tm.ClientListener.OnActivityEventPremiumPassBoughtCalls[0].EventId.Value
			);

			// Level up: 2
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 4, 0, 6, 0));
			AssertEventModel(eventId, xp: 10, totalXp: 20, level: 1);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 1, 3, 1));
			AssertEventModel(eventId, xp: 0, totalXp: 30, level: 2);
			Assert.AreEqual(1, clientListener.OnRewardAddedCallCount);
			Assert.AreEqual(RewardType.ActivityEventLevel, player.Rewards[0].Metadata.Type);
			Assert.AreEqual(1, player.Rewards[0].Items.Count);
			Assert.AreEqual(1, player.Rewards[0].Items[0].Count);
			Assert.AreEqual(2, player.Rewards[0].Items[0].Level);
			Assert.AreEqual(ChainTypeId.FromString("Gem"), player.Rewards[0].Items[0].Type);
			Assert.AreEqual(0, player.Rewards[0].Resources.Count);

			// Level up: 3
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 1, 1, 1));
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 1, 6, 0));
			AssertEventModel(eventId, xp: 0, totalXp: 60, level: 3);

			tm.AnalyticsEventRecorder.Clear();
			tm.AssertAction(new PlayerClaimActivityEventRewards(eventId));
			Assert.AreEqual(7, board.ItemHolder.Count);
			tm.AssertItem("Gold:1", board.ItemHolder[0]); // Reward: level 1 free
			tm.AssertItem("Gem:2", board.ItemHolder[1]); //  Reward: level 1 premium (1st)
			tm.AssertItem("Gem:2", board.ItemHolder[2]); //  Reward: level 1 premium (2nd)
			tm.AssertItem("Gold:2", board.ItemHolder[3]); // Reward: level 2 free
			tm.AssertItem("Gem:3", board.ItemHolder[4]); //  Reward: level 2 premium
			tm.AssertItem("Gold:3", board.ItemHolder[5]); // Reward: level 3 free
			tm.AssertItem("Gem:4", board.ItemHolder[6]); //  Reward: level 3 premium
			Assert.AreEqual(2, clientListener.OnRewardAddedCallCount);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEventRewardsClaimed)));
			claimEvent = (PlayerEventRewardsClaimed)tm.AnalyticsEventRecorder.First(typeof(PlayerEventRewardsClaimed));
			Assert.AreEqual("MondayMergeFest", claimEvent.EventId);
			Assert.AreEqual("Merge", claimEvent.Type);
			Assert.AreEqual(MetaTime.FromDateTime(new DateTime(2022, 5, 16, 4, 0, 0)), claimEvent.EventStartTime);
			Assert.True(claimEvent.HasPremiumPass);
			Assert.AreEqual(1, claimEvent.RewardsClaimedBefore);
			Assert.AreEqual(5, claimEvent.RewardsClaimed);

			// Event over
			tm.ClientListener.OnEventStateChangedCalls.Clear();
			tm.TickProgress(MetaDuration.FromHours(24), 100);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 5, 4, 5));
			AssertEventModel(eventId, xp: 0, totalXp: 60, level: 3);
			Assert.True(eventModel.IsInReview(player.CurrentTime));

			// Check that the finalization event has been sent.
			Assert.AreEqual(1, tm.ClientListener.OnEventStateChangedCalls.Count);
			Assert.AreEqual(
				EventId.FromString("MondayMergeFest"),
				tm.ClientListener.OnEventStateChangedCalls[0].EventId
			);

			tm.AssertAction(new PlayerTerminateActivityEvent(eventId));
			AssertEventModel(eventId, xp: 0, totalXp: 60, level: 3, terminated: true);
		}

		[Test]
		public void ModelInitializedBeforeSecondOccurrence() {
			// Start 1 minute before the start of the 2nd occurrence of "MondayMergeFest" event.
			InitModels(new DateTime(2022, 5, 23, 3, 59, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");

			// Test that the event model is initialized even though player didn't participate in the first round.
			tm.AssertAction(new PlayerPurchaseActivityEventPremiumPass(eventId), ActionResult.InvalidState);
		}

		[Test]
		public void MultipleRewardsAtOnce() {
			// Start in the middle of the event
			InitModels(new DateTime(2022, 5, 16, 9, 30, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 4, 4, 4, 5));
			Assert.AreEqual(4, clientListener.OnRewardAddedCallCount);
			AssertEventModel(eventId, xp: 0, totalXp: 500, level: 5);
			Assert.AreEqual(0, board.ItemHolder.Count); // Rewards not claimed yet

			tm.AssertAction(new PlayerClaimActivityEventRewards(eventId));
			Assert.AreEqual(5, board.ItemHolder.Count);
			tm.AssertItem("Gold:1", board.ItemHolder[0]);
			tm.AssertItem("Gold:2", board.ItemHolder[1]);
			tm.AssertItem("Gold:3", board.ItemHolder[2]);
			tm.AssertItem("Gold:4", board.ItemHolder[3]);
			tm.AssertItem("Gold:5", board.ItemHolder[4]);
			AssertEventModel(eventId, xp: 0, totalXp: 500, level: 5);

			player.Wallet.Gems.Purchased = 150;
			tm.AssertAction(new PlayerPurchaseActivityEventPremiumPass(eventId));
			tm.AssertAction(new PlayerClaimActivityEventRewards(eventId));
			Assert.AreEqual(11, board.ItemHolder.Count);
			tm.AssertItem("Gem:2", board.ItemHolder[5]);
			tm.AssertItem("Gem:2", board.ItemHolder[6]);
			tm.AssertItem("Gem:3", board.ItemHolder[7]);
			tm.AssertItem("Gem:4", board.ItemHolder[8]);
			tm.AssertItem("Gem:5", board.ItemHolder[9]);
			tm.AssertItem("Gem:6", board.ItemHolder[10]);

			ItemDiscovery itemDiscovery = player.Merge.ItemDiscovery;
			Assert.AreEqual(DiscoveryState.Discovered, itemDiscovery.GetState(tm.CreateItemType("Gold:5")));
			Assert.AreEqual(DiscoveryState.Discovered, itemDiscovery.GetState(tm.CreateItemType("Gem:6")));
			AssertEventModel(eventId, xp: 0, totalXp: 500, level: 5, terminated: true);
		}

		[Test]
		public void ClaimDuringReview() {
			// Start in the middle of the event
			InitModels(new DateTime(2022, 5, 16, 4, 30, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");
			ActivityEventModel eventModel = player.ActivityEvents.TryGetState(eventId);
			AssertEventModel(eventId, xp: 0, totalXp: 0, level: 0);

			Assert.False(eventModel.AdSeen);
			tm.AssertAction(new PlayerViewActivityEventAd(eventId));
			Assert.True(eventModel.AdSeen);

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 2, 1, 2));
			AssertEventModel(eventId, xp: 10, totalXp: 20, level: 1);
			Assert.False(eventModel.Terminated);
			Assert.AreEqual(0, eventModel.LastSeenLevel);
			Assert.AreEqual(0, eventModel.LastSeenScore);
			tm.AssertAction(new PlayerUpdateActivityEventLastSeen(eventId));
			Assert.AreEqual(1, eventModel.LastSeenLevel);
			Assert.AreEqual(10, eventModel.LastSeenScore);

			tm.TickProgress(MetaDuration.FromHours(24), 100);
			tm.AssertAction(new PlayerClaimActivityEventRewards(eventId));
			Assert.AreEqual(1, board.ItemHolder.Count);
			tm.AssertItem("Gold:1", board.ItemHolder[0]);
			AssertEventModel(eventId, xp: 10, totalXp: 20, level: 1, terminated: true);

			Assert.False(eventModel.AdSeen);
		}

		[Test]
		public void ClaimDuringEndingSoon() {
			// Start in the middle of the event
			InitModels(new DateTime(2022, 5, 16, 4, 30, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");
			ActivityEventModel eventModel = player.ActivityEvents.TryGetState(eventId);
			AssertEventModel(eventId, xp: 0, totalXp: 0, level: 0);

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 2, 1, 2));
			AssertEventModel(eventId, xp: 10, totalXp: 20, level: 1);

			tm.TickProgress(new DateTime(2022, 5, 17, 3, 30, 0), 100);
			Assert.True(player.Status(eventModel) is MetaActivableVisibleStatus.EndingSoon);
			tm.AssertAction(new PlayerClaimActivityEventRewards(eventId));
			Assert.AreEqual(1, board.ItemHolder.Count);
			tm.AssertItem("Gold:1", board.ItemHolder[0]);
			AssertEventModel(eventId, xp: 10, totalXp: 20, level: 1, terminated: false);
		}

		[Test]
		public void AutoClaimOnSessionStart() {
			// Start in the middle of the event
			InitModels(new DateTime(2022, 5, 16, 4, 30, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 2, 1, 2));
			AssertEventModel(eventId, xp: 10, totalXp: 20, level: 1);
			tm.AnalyticsEventRecorder.Clear();
            CommonUtils.FastForwardTime(player, MetaDuration.FromDays(2));
			player.OnSessionStarted();
			AssertEventModel(eventId, xp: 10, totalXp: 20, level: 1, terminated: true);
			Assert.AreEqual(1, player.Rewards.Count);
			RewardModel reward = player.Rewards[0];
			Assert.AreEqual(RewardType.ActivityEventAutoClaim, reward.Metadata.Type);
			Assert.AreEqual(1, reward.Items.Count);
			ItemCountInfo itemCountInfo = reward.Items[0];
			Assert.AreEqual(1, itemCountInfo.Count);
			Assert.AreEqual(1, itemCountInfo.Level);
			Assert.AreEqual(ChainTypeId.FromString("Gold"), itemCountInfo.Type);

			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEventRewardsClaimed)));
			PlayerEventRewardsClaimed claimEvent =
				(PlayerEventRewardsClaimed)tm.AnalyticsEventRecorder.Events.Find(
					e => e is PlayerEventRewardsClaimed
				);
			Assert.AreEqual("MondayMergeFest", claimEvent.EventId);
			Assert.AreEqual("Merge", claimEvent.Type);
			Assert.False(claimEvent.HasPremiumPass);
			Assert.AreEqual(0, claimEvent.RewardsClaimedBefore);
			Assert.AreEqual(1, claimEvent.RewardsClaimed);
			Assert.True(claimEvent.AutoClaim);
		}

		[Test]
		public void AutoClaimOnActivationStart() {
			// Start in the middle of the event
			InitModels(new DateTime(2022, 5, 16, 4, 30, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 2, 1, 2));
			AssertEventModel(eventId, xp: 10, totalXp: 20, level: 1);
			tm.AnalyticsEventRecorder.Clear();
            // Go past the start of the next occurrence of the event which should reset the model.
            CommonUtils.FastForwardTime(player, MetaDuration.FromDays(8));
			AssertEventModel(eventId, xp: 0, totalXp: 0, level: 0, terminated: false);
			Assert.AreEqual(1, player.Rewards.Count);
			RewardModel reward = player.Rewards[0];
			Assert.AreEqual(RewardType.ActivityEventAutoClaim, reward.Metadata.Type);
			Assert.AreEqual(1, reward.Items.Count);
			ItemCountInfo itemCountInfo = reward.Items[0];
			Assert.AreEqual(1, itemCountInfo.Count);
			Assert.AreEqual(1, itemCountInfo.Level);
			Assert.AreEqual(ChainTypeId.FromString("Gold"), itemCountInfo.Type);

			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerEventRewardsClaimed)));
			PlayerEventRewardsClaimed claimEvent =
				(PlayerEventRewardsClaimed)tm.AnalyticsEventRecorder.Events.Find(
					e => e is PlayerEventRewardsClaimed
				);
			Assert.AreEqual("MondayMergeFest", claimEvent.EventId);
			Assert.AreEqual("Merge", claimEvent.Type);
			Assert.False(claimEvent.HasPremiumPass);
			Assert.AreEqual(0, claimEvent.RewardsClaimedBefore);
			Assert.AreEqual(1, claimEvent.RewardsClaimed);
			Assert.True(claimEvent.AutoClaim);
		}

		[Test]
		public void StartTimeWithUtcOffset() {
			InitModels(new DateTime(2022, 5, 15, 21, 0, 0)); // PlayerModel.CurrentTime is in UTC.
			player.TimeZoneInfo.CurrentUtcOffset = MetaDuration.FromHours(4);
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");

			ActivityEventModel eventModel =
				player.ActivityEvents.SubEnsureHasState(config.ActivityEvents[eventId], player);
			// Progress 3 hours -> 2022-05-16 00:00 UTC (which is 04:00 local time i.e. the start time of the event)
			tm.TickProgress(MetaDuration.FromHours(4), 50);
			// The start time as recorded in the event model should be 2022-05-16 04:00 i.e. the start time
			// in the config independent on the player's timezone. The player's time (UTC) at the start of the event
			// is 2022-05-16 00:00.
			Assert.AreEqual(MetaTime.FromDateTime(new DateTime(2022, 5, 16, 4, 0, 0)), eventModel.StartTime);
		}

		[Test]
		public void StartTimeWithNegativeUtcOffset() {
			InitModels(new DateTime(2022, 5, 16, 0, 0, 0));
			player.TimeZoneInfo.CurrentUtcOffset = MetaDuration.FromHours(-5);
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("MondayMergeFest");

			ActivityEventModel eventModel =
				player.ActivityEvents.SubEnsureHasState(config.ActivityEvents[eventId], player);
			// Progress 9 hours -> 2022-05-16 09:00 UTC (which is 04:00 local time i.e. the start time of the event)
			tm.TickProgress(MetaDuration.FromHours(9), 50);
			Assert.AreEqual(MetaTime.FromDateTime(new DateTime(2022, 5, 16, 4, 0, 0)), eventModel.StartTime);
		}

		private void AssertEventModel(EventId id, int xp, int totalXp, int level, bool terminated = false) {
			ActivityEventModel eventModel = player.ActivityEvents.TryGetState(id);

			Assert.AreEqual(terminated, eventModel.Terminated);
			Assert.AreEqual(xp, eventModel.EventLevel.CurrentXp);
			Assert.AreEqual(totalXp, eventModel.EventLevel.TotalXp);
			Assert.AreEqual(level, eventModel.EventLevel.Level);
		}
	}
}
