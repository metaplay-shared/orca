using Metaplay.Core.Model;

namespace Game.Logic {
    public static class ActionResult {
		// Shadow success system results
		public static readonly MetaActionResult Success = MetaActionResult.Success;

		// Game-specific results
		public static readonly MetaActionResult InvalidCoordinates = new MetaActionResult(nameof(InvalidCoordinates));
		public static readonly MetaActionResult InvalidIndex = new MetaActionResult(nameof(InvalidIndex));
		public static readonly MetaActionResult InvalidParam = new MetaActionResult(nameof(InvalidParam));
		public static readonly MetaActionResult InvalidState = new MetaActionResult(nameof(InvalidState));
		public static readonly MetaActionResult NotEnoughResources = new MetaActionResult(nameof(NotEnoughResources));
		public static readonly MetaActionResult NoItemsLeft = new MetaActionResult(nameof(NoItemsLeft));
		public static readonly MetaActionResult NotEnoughSpace = new MetaActionResult(nameof(NotEnoughSpace));
		public static readonly MetaActionResult NoBuildersAvailable = new MetaActionResult(nameof(NoBuildersAvailable));
		public static readonly MetaActionResult NoSuchEvent = new MetaActionResult(nameof(NoSuchEvent));
		public static readonly MetaActionResult TooManyHeroesInBuilding = new MetaActionResult(nameof(TooManyHeroesInBuilding));
		public static readonly MetaActionResult TooFewHeroesInBuilding = new MetaActionResult(nameof(TooFewHeroesInBuilding));
		public static readonly MetaActionResult IapRequired = new MetaActionResult(nameof(IapRequired));
	}

	public static class ActionCodes {
		public const int PlayerMoveItemOnBoard = 1100;
		public const int PlayerCreateMergeItem = 1101;
		public const int PlayerCollectMergeItem = 1102;
		public const int PlayerFulfillHeroTask = 1103;
		public const int PlayerClaimHeroTaskRewards = 1104;
		public const int PlayerFulfillIslandTask = 1105;
		public const int PlayerPopMergeItem = 1106;
		public const int PlayerUnlockIsland = 1107;
		public const int PlayerClaimBuildingFragment = 1108;
		public const int PlayerFillEnergy = 1109;
		public const int PlayerOpenMergeItem = 1110;
		public const int PlayerSkipHeroTaskTimer = 1111;
		public const int PlayerClaimReward = 1112;
		public const int PlayerClaimBuildingDailyReward = 1113;
		public const int PlayerSkipCreatorTimer = 1114;
		public const int PlayerClaimItemDiscoveryReward = 1115;
		public const int PlayerSellMergeItem = 1116;
		public const int PlayerOpenLockArea = 1117;
		public const int PlayerSkipOpenItemTimer = 1118;
		public const int PlayerRefreshFlashSales = 1120;
		public const int PlayerPurchaseMarketItem = 1121;
		public const int PlayerUpgradeBackpack = 1122;
		public const int PlayerStoreToBackpack = 1123;
		public const int PlayerPopFromBackpack = 1124;
		public const int PlayerInitGame = 1127;
		public const int PlayerEnterIsland = 1128;
		public const int PlayerEnterMap = 1129;
		public const int PlayerUseBuilder = 1130;
		public const int PlayerSkipBuilderTimer = 1131;
		public const int PlayerClaimActivityEventReward = 1132;
		public const int PlayerPurchaseActivityEventPremiumPass = 1133;
		public const int PlayerUpdateActivityEventLastSeen = 1134;
		public const int PlayerViewActivityEventAd = 1135;
		public const int PlayerUseMine = 1136;
		public const int PlayerRepairMine = 1137;
		public const int PlayerClaimMinedItems = 1138;
		public const int PlayerAssignHeroBuilding = 1139;
		public const int PlayerTerminateActivityEvent = 1140;
		public const int PlayerClaimDailyTaskReward = 1141;
		public const int PlayerRevealIsland = 1142;
		public const int PlayerSetSoundSettings = 1143;
		public const int PlayerSkipBuildingDailyTimer = 1144;
		public const int PlayerSendItemToIsland = 1145;
		public const int PlayerOpenBubble = 1146;
		public const int PlayerAcknowledgeBuilding = 1147;
		public const int PlayerOpenLogbookChapter = 1148;
		public const int PlayerClaimLogbookTaskReward = 1149;
		public const int PlayerClaimLogbookChapterReward = 1150;
		public const int PlayerSelectMergeItem = 1151;
		public const int PlayerRegisterShopOpen = 1152;
		public const int PlayerRegisterShopClose = 1153;
		public const int PlayerSetLatestInfoUrl = 1154;
		public const int PlayerForceMismatch = 1155;

		public const int AdminRewardCurrency = 2100;

        public const int GuildPokeMember                                        = 2201;

        public const int GuildSellPokesFinalizingPlayerAction                   = 2202;
        public const int GuildSellPokesFinalizingGuildAction                    = 2203;

        public const int GuildBuyVanityInitiatingPlayerAction                   = 2204;
        public const int GuildBuyVanityCancelingPlayerAction                    = 2205;
        public const int GuildBuyVanityFinalizingGuildAction                    = 2206;

        public const int GuildClaimVanityRankRewardInitiatingPlayerAction       = 2207;
        public const int GuildClaimVanityRankRewardFinalizingPlayerAction       = 2208;
        public const int GuildClaimVanityRankRewardFinalizingGuildAction        = 2209;
        public const int SoftReset        = 2210;
	}

    public static class TransactionPlanCodes
    {
        // \todo: could be made automatic
        public const int GuildSellPokesGuildPlan                    = 1002;
        public const int GuildSellPokesFinalizingPlan               = 1003;

        public const int GuildBuyVanityPlayerPlan                   = 1011;

        public const int GuildClaimVanityRankRewardGuildPlan        = 1022;
        public const int GuildClaimVanityRankRewardFinalizingPlan   = 1023;
    }

    public static class TransactionCodes
    {
        public const int GuildSellPokes             = 101;
        public const int GuildBuyVanity             = 102;
        public const int GuildClaimVanityRankReward = 103;
    }
}
