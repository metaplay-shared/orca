using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Metaplay.Core;
using Metaplay.Core.Activables;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using Metaplay.Core.Schedule;

namespace Game.Logic {

	[MetaSerializableDerived(2)]
	public class ActivityEventsModel : MetaActivableSet<EventId, ActivityEventInfo, ActivityEventModel> {
		protected override ActivityEventModel CreateActivableState(
			ActivityEventInfo info,
			IPlayerModelBase player
		) {
			return new ActivityEventModel(info);
		}

		public ActivityEventModel SubEnsureHasState(ActivityEventInfo info, IPlayerModelBase player) {
			return EnsureHasState(info, player);
		}
	}

	[MetaSerializableDerived(2)]
	public class ActivityEventModel : MetaActivableState<EventId, ActivityEventInfo>, IEventModel {
		[MetaMember(1)] public sealed override EventId ActivableId { get; protected set; }
		[MetaMember(2)] public ActivityEventLevelModel EventLevel { get; protected set; }
		[MetaMember(3)] public int LastSeenLevel { get; protected set; } = 1;
		[MetaMember(4)] public int LastSeenScore { get; protected set; }
		[MetaMember(5)] public MetaTime AdSeenTime { get; protected set; }
		[MetaMember(6)] public MetaTime? PremiumPassPurchase { get; protected set; }
		[MetaMember(7)] public MetaDictionary<int, MetaTime> ClaimedRewardsFree { get; protected set; } = new();
		[MetaMember(8)] public MetaDictionary<int, MetaTime> ClaimedRewardsPremium { get; protected set; } = new();
		[MetaMember(9)] public bool Terminated { get; protected set; }
		[MetaMember(10)] public MetaTime StartTime { get; protected set; }

		[IgnoreDataMember] public ActivityEventInfo Info => ActivableInfo;
		[IgnoreDataMember] public IMetaActivableConfigData<EventId> EventInfo => ActivableInfo;
		[IgnoreDataMember] public string Icon => Info.Icon;
		[IgnoreDataMember] public MetaActivableParams MetaActivableParams => ActivableInfo.ActivableParams;
		[IgnoreDataMember] public int VisualizationOrder => ActivableInfo.VisualizationOrder;
		[IgnoreDataMember] public EventAdMode AdMode => ActivableInfo.AdMode;

		public ActivityEventModel() { }

		public ActivityEventModel(ActivityEventInfo info) : base(info) {
			EventLevel = new ActivityEventLevelModel(ActivableId);
		}

		public override string ToString() {
			return $"{Info.ActivityEventType}:{Info.EventId} {EventLevel.Level}:{EventLevel.CurrentXp}";
		}

		public bool AdSeen => AdSeenTime > MetaTime.Epoch;

		private void Reset(MetaTime startTime) {
			EventLevel.Reset();
			LastSeenLevel = EventLevel.Level;
			LastSeenScore = EventLevel.CurrentXp;
			PremiumPassPurchase = null;
			ClaimedRewardsFree.Clear();
			ClaimedRewardsPremium.Clear();
			Terminated = false;
			StartTime = startTime;
		}

		protected override void OnStartedActivation(IPlayerModelBase player) {
			PlayerModel playerModel = (PlayerModel)player;
			ClaimRewardsToInbox(playerModel, playerModel.CurrentTime, playerModel.ClientListener);
			MetaScheduleOccasion startingOccasion = Info.ActivableParams.Schedule
				.TryGetCurrentOrNextEnabledOccasion(playerModel.GetCurrentLocalTime())
				.Value;
			MetaTime startTime = startingOccasion.EnabledRange.Start + LatestActivation.Value.UtcOffset;
			Reset(startTime);
			playerModel.ClientListener.OnEventStateChanged(ActivableId);
		}

		protected override void Finalize(IPlayerModelBase player) {
			if (Info.ReshowAd) {
				AdSeenTime = MetaTime.Epoch;
			}

			PlayerModel playerModel = (PlayerModel)player;
			playerModel.ClientListener.OnEventStateChanged(ActivableId);
		}

		public List<ItemCountInfo> UnclaimedRewards(SharedGameConfig gameConfig) {
			List<ItemCountInfo> rewards = new List<ItemCountInfo>();
			for (int level = 0; level <= EventLevel.Level; level++) {
				LevelId<EventId> levelId = new LevelId<EventId>(ActivableId, level);
				if (!ClaimedRewardsFree.ContainsKey(level)) {
					ItemCountInfo freeReward = gameConfig.ActivityEventLevels[levelId].FreeRewardItem;
					if (freeReward.Type != ChainTypeId.None) {
						rewards.Add(freeReward);
					}
				}

				if (HasPremiumPass() && !ClaimedRewardsPremium.ContainsKey(level)) {
					ItemCountInfo premiumReward = gameConfig.ActivityEventLevels[levelId].PremiumRewardItem;
					if (premiumReward.Type != ChainTypeId.None) {
						rewards.Add(premiumReward);
					}
				}
			}

			return rewards;
		}

		public int ClaimedRewards() {
			return ClaimedRewardsFree.Count + ClaimedRewardsPremium.Count;
		}

		public int ClaimRewardsToInbox(PlayerModel player, MetaTime time, IPlayerModelClientListener listener) {
			List<ItemCountInfo> unclaimedRewards = UnclaimedRewards(player.GameConfig);
			if (unclaimedRewards.Count == 0) {
				return 0;
			}

			RewardMetadata metadata = new RewardMetadata();
			metadata.Type = RewardType.ActivityEventAutoClaim;
			metadata.Event = ActivableId;
			RewardModel rewardModel = new RewardModel(
				new List<ResourceInfo>(),
				unclaimedRewards,
				ChainTypeId.LevelUpRewards,
				1,
				metadata
			);
			player.Rewards.Add(rewardModel);

			int rewardsClaimedBefore = ClaimedRewards();
			int claimedRewards = MarkRewardsClaimed(time, player.GameConfig, listener);

			player.EventStream.Event(
				new PlayerEventRewardsClaimed(
					ActivableId.Value,
					Info.ActivityEventType.Value,
					HasPremiumPass(),
					rewardsClaimedBefore,
					claimedRewards,
					autoClaim: true,
					StartTime
				)
			);
			return claimedRewards;
		}

		/// <summary>
		/// <c>MarkRewardsClaimed</c> marks all unclaimed rewards as claimed.
		/// </summary>
		/// <param name="time">when the rewards are claimed</param>
		/// <param name="gameConfig">game configuration</param>
		/// <returns>how many rewards was marked claimed</returns>
		public int MarkRewardsClaimed(MetaTime time, SharedGameConfig gameConfig, IPlayerModelClientListener listener) {
			int claimedRewards = 0;
			for (int level = 0; level <= EventLevel.Level; level++) {
				LevelId<EventId> levelId = new LevelId<EventId>(ActivableId, level);
				if (gameConfig.ActivityEventLevels[levelId].FreeRewardItem.Type != ChainTypeId.None) {
					if (ClaimedRewardsFree.AddIfAbsent(level, time)) {
						listener.OnActivityEventRewardClaimed(ActivableId, level, false);
						claimedRewards++;
					}
				}

				if (HasPremiumPass() &&
					gameConfig.ActivityEventLevels[levelId].PremiumRewardItem.Type != ChainTypeId.None) {
					if (ClaimedRewardsPremium.AddIfAbsent(level, time)) {
						listener.OnActivityEventRewardClaimed(ActivableId, level, true);
						claimedRewards++;
					}
				}
			}

			return claimedRewards;
		}

		public void AddScore(
			SharedGameConfig gameConfig,
			int delta,
			Action<RewardModel> rewardHandler,
			IPlayerModelClientListener clientListener,
			IPlayerModelServerListener serverListener,
			ResourceModificationContext context
		) {
			EventLevel.AddXp(gameConfig, delta, rewardHandler, clientListener, serverListener, context);
		}

		public void RefreshLastSeen() {
			LastSeenLevel = EventLevel.Level;
			LastSeenScore = EventLevel.CurrentXp;
		}

		public void MarkAdSeen(MetaTime time) {
			AdSeenTime = time;
		}

		public bool HasPremiumPass() {
			return PremiumPassPurchase.HasValue;
		}

		public void PurchasePremiumPass(MetaTime purchaseTime) {
			PremiumPassPurchase = purchaseTime;
		}

		/// <summary>
		/// <c>Terminate</c> marks the occurrence of an event as terminated which means that the event should not
		/// be displayed to the user anymore irrespective of whether the event is still active or in review (according
		/// to its schedule). Termination is needed when the event provides no more interaction to the player. This can
		/// happen when the player has claimed all the rewards and cannot earn more e.g. because the event is already
		/// in review or there are no more rewards to earn.
		/// </summary>
		public void Terminate() {
			Terminated = true;
		}
	}
}
