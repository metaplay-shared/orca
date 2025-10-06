using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Activables;
using Metaplay.Core.Config;
using Metaplay.Core.Math;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using Metaplay.Core.Schedule;
using static System.FormattableString;

namespace Game.Logic {
	[MetaSerializable]
	public class DiscountEventType : StringId<DiscountEventType> {
		public static readonly DiscountEventType None = FromString("None");
		public static readonly DiscountEventType Energy = FromString("Energy");
		public static readonly DiscountEventType BuilderTimer = FromString("BuilderTimer");
		public static readonly DiscountEventType BuilderTimerGold = FromString("BuilderTimerGold");
	}

	/// <summary>
	/// <c>DiscountEventInfo</c> describes a scheduled in-game event that has a discount associated to it. For example,
	/// an "energy discount" event will increase the energy production rate for the duration of the event.
	///
	/// <c>DiscountEventType</c> specifies the concrete type of the event and dictates which parameters impact
	/// the game mechanics. For example, an event of type "Energy" uses <c>EnergyProductionFactor</c> to modify
	/// the energy production rate.
	/// </summary>
	[MetaSerializable]
	public class DiscountEventInfo : IMetaActivableConfigData<EventId> {
		[MetaMember(1)] public EventId EventId { get; private set; }
		[MetaMember(2)] public DiscountEventType DiscountEventType { get; private set; }
		[MetaMember(3)] public string DisplayName { get; private set; }
		[MetaMember(4)] public string Description { get; private set; }
		[MetaMember(5)] public MetaActivableParams ActivableParams { get; private set; }
		[MetaMember(6)] public bool ReshowAd { get; private set; }

		// Event type specific parameters
		[MetaMember(7)] public F64 EnergyProductionFactor { get; private set; }
		[MetaMember(8)] public F64 BuilderTimerFactor { get; private set; }
		[MetaMember(9)] public string Icon { get; private set; }
		[MetaMember(10)] public int VisualizationOrder { get; private set; }
		[MetaMember(11)] public EventAdMode AdMode { get; private set; }

		public EventId ActivableId => EventId;
		public EventId ConfigKey => EventId;

		public string DisplayShortInfo {
			get {
				if (DiscountEventType.Energy == DiscountEventType) {
					return Invariant($"{DiscountEventType.Energy.Value}:{EnergyProductionFactor.Float:0.00}");
				} else if (DiscountEventType.BuilderTimer == DiscountEventType) {
					return Invariant($"{DiscountEventType.BuilderTimer.Value}:{BuilderTimerFactor.Float:0.00}");
				} else {
					return DiscountEventType.Value;
				}
			}
		}

		public DiscountEventInfo() { }

		public DiscountEventInfo(
			EventId eventId,
			DiscountEventType discountEventType,
			string displayName,
			string description,
			F64 energyProductionFactor,
			F64 builderTimerFactor,
			bool reshowAd,
			MetaActivableParams activableParams,
			string icon,
			int visualizationOrder,
			EventAdMode adMode
		) {
			EventId = eventId ??
				throw new ArgumentNullException(nameof(eventId));
			DiscountEventType = discountEventType ?? throw new ArgumentNullException(nameof(discountEventType));
			DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
			Description = description ?? throw new ArgumentNullException(nameof(description));
			EnergyProductionFactor = energyProductionFactor;
			BuilderTimerFactor = builderTimerFactor;
			ReshowAd = reshowAd;
			ActivableParams = activableParams ?? throw new ArgumentNullException(nameof(activableParams));
			Icon = icon;
			VisualizationOrder = visualizationOrder;
			AdMode = adMode;
		}
	}

	/// <summary>
	/// <c>DiscountEventConfigItem</c> corresponds the rows of the "DiscountEvents" tab of the game configuration
	/// Google sheet.
	/// </summary>
	public class DiscountEventConfigItem : IGameConfigSourceItem<EventId, DiscountEventInfo> {
		public EventId EventId;
		public DiscountEventType DiscountEventType;
		public string DisplayName;
		public string Description;
		public MetaScheduleBase Schedule;
		public bool ReshowAd;

		// Event type specific parameters
		public F64 EnergyProductionFactor;
		public F64 BuilderTimerFactor;
		public string Icon;
		public int VisualizationOrder;
		public EventAdMode AdMode;

		public EventId ConfigKey => EventId;

		public DiscountEventInfo ToConfigData(GameConfigBuildLog buildLog) {
			return new DiscountEventInfo(
				eventId: EventId,
				discountEventType: DiscountEventType,
				displayName: DisplayName,
				description: Description,
				energyProductionFactor: EnergyProductionFactor,
				builderTimerFactor: BuilderTimerFactor,
				reshowAd: ReshowAd,
				activableParams: new MetaActivableParams(
					isEnabled: true,
					segments: new List<MetaRef<PlayerSegmentInfoBase>>(),
					additionalConditions: new List<PlayerCondition> { new DiscountEventPlayerCondition() },
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

	[MetaSerializableDerived(1)]
	public class DiscountEventPlayerCondition : PlayerCondition {
		public override bool MatchesPlayer(IPlayerModelBase player) {
			PlayerModel playerModel = (PlayerModel)player;
			return playerModel.PrivateProfile.FeaturesEnabled.Contains(FeatureTypeId.DiscountEvents);
		}

		public override IEnumerable<PlayerSegmentId> GetSegmentReferences() {
			return new List<PlayerSegmentId>();
		}
	}
}
