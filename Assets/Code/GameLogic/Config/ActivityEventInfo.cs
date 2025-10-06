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
	public class ActivityEventType : StringId<ActivityEventType> {
		public static readonly ActivityEventType None = FromString("None");
		public static readonly ActivityEventType Merge = FromString("Merge");
		public static readonly ActivityEventType Build = FromString("Build");
		public static readonly ActivityEventType HeroTasks = FromString("HeroTasks");
		public static readonly ActivityEventType Chests = FromString("Chests");
	}

	/// <summary>
	/// <c>ActivityEventInfo</c> describes a scheduled in-game event with the goal of collecting score by completing
	/// event specific actions. For example, in a "merge collect" event the player earns score by merging items.
	/// Merging higher level items will earn higher scores. The score is mapped to a set of rewards modeled as
	/// <see cref="ActivityEventLevelInfo"/>.
	/// </summary>
	[MetaSerializable]
	public class ActivityEventInfo : IMetaActivableConfigData<EventId> {
		[MetaMember(1)] public EventId EventId { get; private set; }
		[MetaMember(2)] public ActivityEventType ActivityEventType { get; private set; }
		[MetaMember(3)] public string DisplayName { get; private set; }
		[MetaMember(4)] public string Description { get; private set; }
		[MetaMember(5)] public ResourceInfo PremiumPassPrice { get; private set; }
		[MetaMember(6)] public MetaActivableParams ActivableParams { get; private set; }
		[MetaMember(7)] public bool ReshowAd { get; private set; }
		[MetaMember(8)] public string Icon { get; private set; }
		[MetaMember(9)] public int VisualizationOrder { get; private set; }
		[MetaMember(10)] public EventAdMode AdMode { get; private set; }

		public EventId ActivableId => EventId;
		public EventId ConfigKey => EventId;

		public string DisplayShortInfo => $"{EventId}, type: {ActivityEventType.Value}";

		public ActivityEventInfo() { }

		public ActivityEventInfo(
			EventId eventId,
			ActivityEventType activityEventType,
			string displayName,
			string description,
			ResourceInfo premiumPassPrice,
			bool reshowAd,
			MetaActivableParams activableParams,
			string icon,
			int visualizationOrder,
			EventAdMode adMode
		) {
			EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
			ActivityEventType = activityEventType ?? throw new ArgumentNullException(nameof(activityEventType));
			DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
			Description = description ?? throw new ArgumentNullException(nameof(description));
			PremiumPassPrice = premiumPassPrice ?? throw new ArgumentNullException(nameof(premiumPassPrice));
			ReshowAd = reshowAd;
			ActivableParams = activableParams ?? throw new ArgumentNullException(nameof(activableParams));
			Icon = icon ?? throw new ArgumentNullException(nameof(icon));
			VisualizationOrder = visualizationOrder;
			AdMode = adMode;
		}
	}

	/// <summary>
	/// <c>ActivityEventConfigItem</c> corresponds to a row of the "ActivityEvents" tab of the game configuration
	/// Google sheet.
	/// </summary>
	public class ActivityEventConfigItem : IGameConfigSourceItem<EventId, ActivityEventInfo> {
		public EventId EventId;
		public ActivityEventType ActivityEventType;
		public string DisplayName;
		public string Description;
		public ResourceInfo PremiumPassPrice;
		public MetaScheduleBase Schedule;
		public bool ReshowAd;
		public string Icon;
		public int VisualizationOrder;
		public EventAdMode AdMode;

		public EventId ConfigKey => EventId;

		public ActivityEventInfo ToConfigData(GameConfigBuildLog buildLog) {
			return new ActivityEventInfo(
				eventId: EventId,
				activityEventType: ActivityEventType,
				displayName: DisplayName,
				description: Description,
				premiumPassPrice: PremiumPassPrice,
				reshowAd: ReshowAd,
				activableParams: new MetaActivableParams(
					isEnabled: true,
					segments: new List<MetaRef<PlayerSegmentInfoBase>>(),
					additionalConditions: new List<PlayerCondition> { new ActivityEventPlayerCondition() },
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

	[MetaSerializableDerived(2)]
	public class ActivityEventPlayerCondition : PlayerCondition {
		public override bool MatchesPlayer(IPlayerModelBase player) {
			PlayerModel playerModel = (PlayerModel)player;
			return playerModel.PrivateProfile.FeaturesEnabled.Contains(FeatureTypeId.ActivityEvents);
		}

		public override IEnumerable<PlayerSegmentId> GetSegmentReferences() {
			return new List<PlayerSegmentId>();
		}
	}
}
