using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Activables;
using Metaplay.Core.Config;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using Metaplay.Core.Schedule;

namespace Game.Logic {
	[MetaSerializable]
	public class DailyTaskEventInfo : IMetaActivableConfigData<EventId> {
		[MetaMember(1)] public EventId EventId { get; private set; }
		[MetaMember(2)] public DailyTaskSetId DailyTaskSetId { get; private set; }
		[MetaMember(3)] public string DisplayName { get; private set; }
		[MetaMember(4)] public string Description { get; private set; }
		[MetaMember(5)] public MetaActivableParams ActivableParams { get; private set; }

		[MetaMember(6)] public int LevelPenalty { get; private set; }
		[MetaMember(7)] public int LevelPenaltyRepeats { get; private set; }
		[MetaMember(8)] public List<LevelId<ChainTypeId>> Rewards;
		[MetaMember(9)] public bool ReshowAd;
		[MetaMember(10)] public string Icon { get; private set; }
		[MetaMember(11)] public int VisualizationOrder { get; private set; }
		[MetaMember(12)] public EventAdMode AdMode { get; private set; }

		public EventId ActivableId => EventId;
		public EventId ConfigKey => EventId;
		public int MaxLevel => Rewards.Count - 1;

		public string DisplayShortInfo => $"{EventId}, task set {DailyTaskSetId}";

		public DailyTaskEventInfo() { }

		public DailyTaskEventInfo(
			EventId eventId,
			DailyTaskSetId dailyTaskSetId,
			string displayName,
			string description,
			int levelPenalty,
			int levelPenaltyRepeats,
			List<LevelId<ChainTypeId>> rewards,
			bool reshowAd,
			MetaActivableParams activableParams,
			string icon,
			int visualizationOrder,
			EventAdMode adMode
		) {
			EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
			DailyTaskSetId = dailyTaskSetId ?? throw new ArgumentNullException(nameof(dailyTaskSetId));
			DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
			Description = description ?? throw new ArgumentNullException(nameof(description));
			LevelPenalty = levelPenalty;
			LevelPenaltyRepeats = levelPenaltyRepeats;
			Rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
			ActivableParams = activableParams ?? throw new ArgumentNullException(nameof(activableParams));
			ReshowAd = reshowAd;
			Icon = icon;
			VisualizationOrder = visualizationOrder;
			AdMode = adMode;
		}
	}

	public class DailyTaskEventConfigItem : IGameConfigSourceItem<EventId, DailyTaskEventInfo> {
		public EventId EventId;
		public DailyTaskSetId DailyTaskSetId;
		public string DisplayName;
		public string Description;
		public MetaScheduleBase Schedule;

		public int LevelPenalty;
		public int LevelPenaltyRepeats;
		public List<LevelId<ChainTypeId>> Rewards;
		public bool ReshowAd;
		public string Icon;
		public int VisualizationOrder;
		public EventAdMode AdMode;

		public EventId ConfigKey => EventId;

		public DailyTaskEventInfo ToConfigData(GameConfigBuildLog buildLog) {
			return new DailyTaskEventInfo(
				eventId: EventId,
				dailyTaskSetId: DailyTaskSetId,
				displayName: DisplayName,
				description: Description,
				levelPenalty: LevelPenalty,
				levelPenaltyRepeats: LevelPenaltyRepeats,
				rewards: Rewards,
				reshowAd: ReshowAd,
				activableParams: new MetaActivableParams(
					isEnabled: true,
					segments: new List<MetaRef<PlayerSegmentInfoBase>>(),
					additionalConditions: new List<PlayerCondition> { new DailyTaskEventPlayerCondition() },
					lifetime: MetaActivableLifetimeSpec.ScheduleBased.Instance,
					isTransient: false,
					schedule: Schedule,
					maxActivations: null,
					maxTotalConsumes: null,
					maxConsumesPerActivation: null,
					cooldown: MetaActivableCooldownSpec.ScheduleBased.Instance,
					allowActivationAdjustment: true
				),
				icon: Icon,
				visualizationOrder: VisualizationOrder,
				adMode: AdMode
			);
		}
	}

	[MetaSerializableDerived(4)]
	public class DailyTaskEventPlayerCondition : PlayerCondition {
		public override bool MatchesPlayer(IPlayerModelBase player) {
			PlayerModel playerModel = (PlayerModel)player;
			return playerModel.PrivateProfile.FeaturesEnabled.Contains(FeatureTypeId.DailyTaskEvents);
		}

		public override IEnumerable<PlayerSegmentId> GetSegmentReferences() {
			return new List<PlayerSegmentId>();
		}
	}
}
