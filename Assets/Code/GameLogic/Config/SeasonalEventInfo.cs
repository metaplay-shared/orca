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
	public class SeasonalEventInfo : IMetaActivableConfigData<EventId> {
		[MetaMember(1)] public EventId EventId { get; private set; }
		[MetaMember(2)] public ChainTypeId ItemType { get; private set; }
		[MetaMember(3)] public ChainTypeId ChestType { get; private set; }
		[MetaMember(4)] public string DisplayName { get; private set; }
		[MetaMember(5)] public string Description { get; private set; }
		[MetaMember(6)] public MetaActivableParams ActivableParams { get; private set; }
		[MetaMember(7)] public bool ReshowAd { get; private set; }
		[MetaMember(8)] public string Icon { get; private set; }
		[MetaMember(9)] public int VisualizationOrder { get; private set; }
		[MetaMember(10)] public EventAdMode AdMode { get; private set; }

		public EventId ActivableId => EventId;
		public EventId ConfigKey => EventId;

		public string DisplayShortInfo => $"{EventId}";

		public SeasonalEventInfo() { }

		public SeasonalEventInfo(
			EventId eventId,
			ChainTypeId chestType,
			ChainTypeId itemType,
			string displayName,
			string description,
			MetaActivableParams activableParams,
			bool reshowAd,
			string icon,
			int visualizationOrder,
			EventAdMode adMode
		) {
			EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
			ChestType = chestType ?? throw new ArgumentNullException(nameof(chestType));
			ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
			DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
			Description = description ?? throw new ArgumentNullException(nameof(description));
			ActivableParams = activableParams ?? throw new ArgumentNullException(nameof(activableParams));
			ReshowAd = reshowAd;
			Icon = icon;
			VisualizationOrder = visualizationOrder;
			AdMode = adMode;
		}
	}

	public class SeasonalEventConfigItem : IGameConfigSourceItem<EventId, SeasonalEventInfo> {
		public EventId EventId;
		public ChainTypeId ChestType;
		public ChainTypeId ItemType;
		public string DisplayName;
		public string Description;
		public MetaScheduleBase Schedule;
		public bool ReshowAd;
		public string Icon;
		public int VisualizationOrder;
		public EventAdMode AdMode;

		public EventId ConfigKey => EventId;

		public SeasonalEventInfo ToConfigData(GameConfigBuildLog buildLog) {
			return new SeasonalEventInfo(
				eventId: EventId,
				chestType: ChestType,
				itemType: ItemType,
				displayName: DisplayName,
				description: Description,
				activableParams: new MetaActivableParams(
					isEnabled: true,
					segments: new List<MetaRef<PlayerSegmentInfoBase>>(),
					additionalConditions: new List<PlayerCondition> { new SeasonalEventPlayerCondition() },
					lifetime: MetaActivableLifetimeSpec.ScheduleBased.Instance,
					isTransient: false,
					schedule: Schedule,
					maxActivations: null,
					maxTotalConsumes: null,
					maxConsumesPerActivation: null,
					cooldown: MetaActivableCooldownSpec.ScheduleBased.Instance,
					allowActivationAdjustment: true
				),
				reshowAd: ReshowAd,
				icon: Icon,
				visualizationOrder: VisualizationOrder,
				adMode: AdMode
			);
		}
	}

	[MetaSerializableDerived(5)]
	public class SeasonalEventPlayerCondition : PlayerCondition {
		public override bool MatchesPlayer(IPlayerModelBase player) {
			PlayerModel playerModel = (PlayerModel)player;
			return playerModel.PrivateProfile.FeaturesEnabled.Contains(FeatureTypeId.SeasonalEvents);
		}

		public override IEnumerable<PlayerSegmentId> GetSegmentReferences() {
			return new List<PlayerSegmentId>();
		}
	}
}
