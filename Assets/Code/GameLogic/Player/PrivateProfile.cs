using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class FeatureTypeId : StringId<FeatureTypeId> {
		public static readonly FeatureTypeId None = FromString("None");
		public static readonly FeatureTypeId HomeIsland = FromString("HomeIsland");
		public static readonly FeatureTypeId Map = FromString("Map");
		public static readonly FeatureTypeId DiscountEvents = FromString("DiscountEvents");
		public static readonly FeatureTypeId ActivityEvents = FromString("ActivityEvents");
		public static readonly FeatureTypeId DailyTaskEvents = FromString("DailyTaskEvents");
		public static readonly FeatureTypeId SeasonalEvents = FromString("SeasonalEvents");
		public static readonly FeatureTypeId Shop = FromString("Shop");
		public static readonly FeatureTypeId Logbook = FromString("Logbook");
		public static readonly FeatureTypeId ItemRemoval = FromString("ItemRemoval");
		public static readonly FeatureTypeId HudButtonGold = FromString("HudButtonGold");
		public static readonly FeatureTypeId HudButtonGems = FromString("HudButtonGems");
		public static readonly FeatureTypeId HudButtonEnergy = FromString("HudButtonEnergy");
		public static readonly FeatureTypeId HudButtonBuilders = FromString("HudButtonBuilders");
		public static readonly FeatureTypeId HudButtonIslandTokens = FromString("HudButtonIslandTokens");

		public bool IsEvent =>
			this == DiscountEvents || this == ActivityEvents || this == DailyTaskEvents || this == SeasonalEvents;
	}

	[MetaSerializable]
	public class SoundSettings {
		[MetaMember(1)] public bool SoundEnabled { get; set; } = false;
		[MetaMember(2)] public bool MusicEnabled { get; set; } = false;
	}

	[MetaSerializable]
	public class PrivateProfile {
		[MetaMember(1)] public SoundSettings SoundSettings { get; set; } = new();
		[MetaMember(2)] public OrderedSet<FeatureTypeId> FeaturesEnabled { get; set; } = new();

		/*
		 * Planned IDs
		 *
		 * 1 audio settings
		 * 2 unlocked features
		 */
	}
}
