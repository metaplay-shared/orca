using Metaplay.Core;
using Metaplay.Core.Forms;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	[MetaFormConfigLibraryItemEntries(new [] { "Gold", "Gems", "Energy", "IslandTokens", "TrophyTokens", "Xp", "Time", "Hero", "Builders" })]
	public class CurrencyTypeId : StringId<CurrencyTypeId> {
		public static readonly CurrencyTypeId Gold = FromString("Gold");
		public static readonly CurrencyTypeId Gems = FromString("Gems");
		public static readonly CurrencyTypeId Energy = FromString("Energy");
		public static readonly CurrencyTypeId IslandTokens = FromString("IslandTokens");
		public static readonly CurrencyTypeId TrophyTokens = FromString("TrophyTokens");
		public static readonly CurrencyTypeId Xp = FromString("Xp");
		public static readonly CurrencyTypeId Time = FromString("Time");
		public static readonly CurrencyTypeId Hero = FromString("Hero");
		public static readonly CurrencyTypeId Builders = FromString("Builders");
		public static readonly CurrencyTypeId MergeEvent = FromString("MergeEvent");
		public static readonly CurrencyTypeId None = FromString("None");

		public bool WalletResource => this == Gems || this == Gold || this == IslandTokens || this == TrophyTokens;
	}

	[MetaSerializable]
	[MetaFormConfigLibraryItemEntries(new []{"MainIsland", "EnergyIsland", "TrophyIsland"})]
	public class IslandTypeId : StringId<IslandTypeId> {
		public static readonly IslandTypeId None = FromString("None");
		public static readonly IslandTypeId All = FromString("All");
		public static readonly IslandTypeId MainIsland = FromString("MainIsland");
		public static readonly IslandTypeId EnergyIsland = FromString("EnergyIsland");
		public static readonly IslandTypeId TrophyIsland = FromString("TrophyIsland");
	}

	[MetaSerializable]
	public class IslanderId : StringId<IslanderId> { }

	[MetaSerializable]
	public class HeroTypeId : StringId<HeroTypeId> {
		public static readonly HeroTypeId None = FromString("None");
	}

	[MetaSerializable]
	public class ChainTypeId : StringId<ChainTypeId> {
		public static readonly ChainTypeId None = FromString("None");
		public static readonly ChainTypeId ResourceItem = FromString("ResourceItem");
		public static readonly ChainTypeId ResourceCreator = FromString("ResourceCreator");
		public static readonly ChainTypeId HeroItem = FromString("HeroItem");
		public static readonly ChainTypeId BuildingItem = FromString("BuildingItem");
		public static readonly ChainTypeId IslandCreator = FromString("IslandCreator");
		public static readonly ChainTypeId IslandChest = FromString("IslandChest");
		public static readonly ChainTypeId IslandRewards = FromString("IslandRewards");
		public static readonly ChainTypeId LevelUpRewards = FromString("LevelUpRewards");
		public static readonly ChainTypeId IslandToken = FromString("IslandToken");
		public static readonly ChainTypeId Gold = FromString("Gold");
		public static readonly ChainTypeId InAppPurchase = FromString("InAppPurchase");
	}

	[MetaSerializable]
	public class CategoryId : StringId<CategoryId> { }

	[MetaSerializable]
	public class CreatorTypeId : StringId<CreatorTypeId> {
		public static readonly CreatorTypeId None = FromString("None");
	}

	[MetaSerializable]
	public class ConverterTypeId : StringId<ConverterTypeId> {
		public static readonly ConverterTypeId None = FromString("None");
	}

	[MetaSerializable]
	public class MineTypeId : StringId<MineTypeId> {
		public static readonly MineTypeId None = FromString("None");
	}

	[MetaSerializable]
	public class BoosterTypeId : StringId<BoosterTypeId> {
		public static readonly BoosterTypeId None = FromString("None");
	}

	[MetaSerializable]
	public class TimerTypeId : StringId<TimerTypeId> {
		public static readonly TimerTypeId SkipOpenItemTimer = FromString("SkipOpenItemTimer");
		public static readonly TimerTypeId SkipHeroTaskTimer = FromString("SkipHeroTaskTimer");
		public static readonly TimerTypeId SkipCreatorTimer = FromString("SkipCreatorTimer");
		public static readonly TimerTypeId SkipBuilderTimer = FromString("SkipBuilderTimer");
		public static readonly TimerTypeId SkipBuilderTimerGold = FromString("SkipBuilderTimerGold");
		public static readonly TimerTypeId SkipBuildingDailyTimer = FromString("SkipBuildingDailyTimer");
	}

	[MetaSerializable]
	public class ShopCategoryId : StringId<ShopCategoryId> {
		public static readonly ShopCategoryId Gems = FromString("Gems");
		public static readonly ShopCategoryId Gold = FromString("Gold");
		public static readonly ShopCategoryId Energy = FromString("Energy");
		public static readonly ShopCategoryId Offer = FromString("Offer");
		public static readonly ShopCategoryId None = FromString("None");
	}

	[MetaSerializable]
	public class ShopItemId : StringId<ShopItemId> {
		public static readonly ShopItemId None = FromString("None");
	}

	[MetaSerializable]
	public class VipPassId : StringId<VipPassId> {
		public static readonly VipPassId None = FromString("None");
	}

	[MetaSerializable]
	public class TriggerId : StringId<TriggerId> { }

	[MetaSerializable]
	public class DialogueId : StringId<DialogueId> {
		public static readonly DialogueId None = FromString("None");
	}

	[MetaSerializable]
	public class BuilderActionId : StringId<BuilderActionId> {
		public static readonly BuilderActionId Build = FromString("Build");
		public static readonly BuilderActionId Mine = FromString("Mine");
		public static readonly BuilderActionId RepairMine = FromString("RepairMine");
	}

	[MetaSerializable]
	public class EconomyActionId : StringId<EconomyActionId> {
		public static readonly EconomyActionId FillEnergy = FromString("FillEnergy");
		public static readonly EconomyActionId ItemCollected = FromString("ItemCollected");
		public static readonly EconomyActionId LogbookRewardClaimed = FromString("LogbookRewardClaimed");
		public static readonly EconomyActionId HeroTaskTimerSkipped = FromString("HeroTaskTimerSkipped");
		public static readonly EconomyActionId CreatorTimerSkipped = FromString("CreatorTimerSkipped");
		public static readonly EconomyActionId OpenItemTimerSkipped = FromString("OpenItemTimerSkipped");
		public static readonly EconomyActionId FlashSalesRefreshed = FromString("FlashSalesRefreshed");
		public static readonly EconomyActionId ItemSold = FromString("ItemSold");
		public static readonly EconomyActionId BackpackUpgraded = FromString("BackpackUpgraded");
		public static readonly EconomyActionId IslandUnlocked = FromString("IslandUnlocked");
		public static readonly EconomyActionId AreaUnlocked = FromString("AreaUnlocked");
		public static readonly EconomyActionId FlashSaleItemPurchased = FromString("FlashSaleItemPurchased");
		public static readonly EconomyActionId ShopItemPurchased = FromString("ShopItemPurchased");
		public static readonly EconomyActionId BuilderTimerSkipped = FromString("BuilderTimerSkipped");
		public static readonly EconomyActionId ActivityEventPremiumPassPurchased =
			FromString("ActivityEventPremiumPassPurchased");
		public static readonly EconomyActionId AssignHero = FromString("AssignHero");
		public static readonly EconomyActionId BuildingDailyTimerSkipped = FromString("BuildingDailyTimerSkipped");
		public static readonly EconomyActionId BubbleOpened = FromString("BubbleOpened");
	}

	[MetaSerializable]
	public class SelectActionId : StringId<SelectActionId> {
		public static readonly SelectActionId None = FromString("None");
		public static readonly SelectActionId HeroPopup = FromString("HeroPopup");
		public static readonly SelectActionId BuildingPopup = FromString("BuildingPopup");
	}

	[MetaSerializable]
	public class EventId : StringId<EventId> { }

	[MetaSerializable]
	public class EventAdMode : StringId<EventAdMode> {
		public static readonly EventAdMode None = FromString("None");
		public static readonly EventAdMode OnPreview = FromString("OnPreview");
		public static readonly EventAdMode OnActive = FromString("OnActive");
		public static readonly EventAdMode Always = FromString("Always");
	}

	[MetaSerializable]
	public class DailyTaskTypeId : StringId<DailyTaskTypeId> {
		public static readonly DailyTaskTypeId Merge = FromString("Merge");
		public static readonly DailyTaskTypeId MergeIslandToken = FromString("MergeIslandToken");
		public static readonly DailyTaskTypeId CollectResources = FromString("CollectResources");
		public static readonly DailyTaskTypeId UseGems = FromString("UseGems");
		public static readonly DailyTaskTypeId UseGold = FromString("UseGold");
		public static readonly DailyTaskTypeId OpenChest = FromString("OpenChest");
		public static readonly DailyTaskTypeId CompleteHeroTask = FromString("CompleteHeroTask");
		public static readonly DailyTaskTypeId CollectGold = FromString("CollectGold");
	}

	[MetaSerializable]
	public class DailyTaskSetId : StringId<DailyTaskSetId> { }
}
