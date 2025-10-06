using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Analytics;
using Metaplay.Core.Math;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using static System.FormattableString;

namespace Game.Logic {
	public static class PlayerEventCodes {
		public const int WalletBalance = 2001;
		public const int LevelUp = 2002;
		public const int MergeItemCollected = 2003;
		public const int MergeItemPopped = 2004;
		public const int MergeItemOpened = 2005;
		public const int BuildingDailyRewardClaimed = 2012;
		public const int BuildingFragmentClaimed = 2013;
		public const int HeroTaskRewardsClaimed = 2014;
		public const int ItemDiscoveryRewardClaimed = 2015;
		public const int RewardClaimed = 2016;
		public const int HeroUnlocked = 2017;
		public const int HeroTaskFulfilled = 2018;
		public const int IslandTaskFulfilled = 2019;
		public const int BoosterItemUsed = 2020;
		public const int TriggerExecuted = 2023;
		public const int StoredToBackpack = 2025;
		public const int PoppedFromBackpack = 2026;
		public const int IslandRemoved = 2028;
		public const int EconomyAction = 2029;
		public const int ItemDiscovered = 2030;
		public const int IslandEntered = 2031;
		public const int MapEntered = 2032;
		public const int BuilderUsed = 2033;
		public const int EventRewardsClaimed = 2034;
		public const int EventAdSeen = 2035;
		public const int MinedItemsClaimed = 2036;
		public const int IslandRevealed = 2037;
		public const int LogbookTaskRewardClaimed = 2038;
		public const int LogbookChapterRewardClaimed = 2039;
		public const int AdminRewardCurrency = 2040;
	}

	[AnalyticsEvent(PlayerEventCodes.EconomyAction)]
	public class PlayerEconomyAction : PlayerEventBase {
		[MetaMember(1)] public int PlayerLevel { get; private set; }
		[MetaMember(2)] public int EarnedGems { get; private set; }
		[MetaMember(3)] public int PurchasedGems { get; private set; }
		[MetaMember(4)] public int EarnedGold { get; private set; }
		[MetaMember(5)] public int PurchasedGold { get; private set; }
		[MetaMember(6)] public EconomyActionId Action { get; private set; }
		[MetaMember(7)] public CurrencyTypeId CostType { get; private set; }
		[MetaMember(8)] public int CostAmount { get; private set; }
		[MetaMember(9)] public string RealCostType { get; private set; }
		[MetaMember(10)] public int RealCostAmount { get; private set; }
		[MetaMember(11)] public CurrencyTypeId GainType { get; private set; }
		[MetaMember(12)] public int GainAmount { get; private set; }
		[MetaMember(13)] public string ProductId { get; private set; }

		public PlayerEconomyAction() { }

		public PlayerEconomyAction(
			PlayerModel player,
			EconomyActionId action,
			CurrencyTypeId costType,
			int costAmount,
			string realCostType,
			int realCostAmount,
			CurrencyTypeId gainType,
			int gainAmount,
			string productId
		) {
			PlayerLevel = player.PlayerLevel;
			EarnedGems = player.Wallet.Gems.Earned;
			PurchasedGems = player.Wallet.Gems.Purchased;
			EarnedGold = player.Wallet.Gold.Earned;
			PurchasedGold = player.Wallet.Gold.Purchased;
			Action = action;
			CostType = costType;
			CostAmount = costAmount;
			RealCostType = realCostType;
			RealCostAmount = realCostAmount;
			GainType = gainType;
			GainAmount = gainAmount;
			ProductId = productId;
		}

		public override string EventDescription =>
			Invariant(
				$"Level {PlayerLevel}, Gems ({EarnedGems} + {PurchasedGems}), Gold ({EarnedGold} + {PurchasedGold}), Action {Action}, CostType {CostType}, CostAmount {CostAmount}, RealCostType {RealCostType}, RealCostAmount {RealCostAmount}, GainType {GainType}, GainAmount {GainAmount}, ProductId {ProductId}"
			);
	}

	[AnalyticsEvent(PlayerEventCodes.WalletBalance)]
	public class PlayerEventWalletBalance : PlayerEventBase {
		[MetaMember(1)] public int EarnedGems { get; private set; }
		[MetaMember(2)] public int PurchasedGems { get; private set; }
		[MetaMember(3)] public int EarnedGold { get; private set; }
		[MetaMember(4)] public int PurchasedGold { get; private set; }

		public PlayerEventWalletBalance() { }

		public PlayerEventWalletBalance(PlayerWalletModel wallet) {
			EarnedGems = wallet.Gems.Earned;
			PurchasedGems = wallet.Gems.Purchased;
			EarnedGold = wallet.Gold.Earned;
			PurchasedGold = wallet.Gold.Purchased;
		}

		public override string EventDescription =>
			Invariant($"Gems ({EarnedGems} + {PurchasedGems}), Gold ({EarnedGold} + {PurchasedGold})");
	}

	[AnalyticsEvent(PlayerEventCodes.LevelUp)]
	public class PlayerLevelUp : PlayerEventBase {
		[MetaMember(1)] public int Level { get; private set; }

		public PlayerLevelUp() { }

		public PlayerLevelUp(int level) {
			Level = level;
		}

		public override string EventDescription => Invariant($"Level {Level}");
	}

	[AnalyticsEvent(PlayerEventCodes.MergeItemCollected)]
	public class PlayerMergeItemCollected : PlayerEventBase {
		[MetaMember(1)] public ChainTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public CurrencyTypeId EarnType { get; private set; }
		[MetaMember(4)] public int Value { get; private set; }

		public PlayerMergeItemCollected() { }

		public PlayerMergeItemCollected(ChainTypeId type, int level, CurrencyTypeId earnType, int value) {
			Type = type;
			Level = level;
			EarnType = earnType;
			Value = value;
		}

		public override string EventDescription =>
			Invariant($"Type {Type}, Level {Level}, EarnType {EarnType}, Value {Value}");
	}

	[AnalyticsEvent(PlayerEventCodes.MergeItemPopped)]
	public class PlayerMergeItemPopped : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId IslandId { get; private set; }
		[MetaMember(2)] public ChainTypeId Type { get; private set; }
		[MetaMember(3)] public int Level { get; private set; }
		[MetaMember(4)] public int HolderSize { get; private set; }

		public PlayerMergeItemPopped() { }

		public PlayerMergeItemPopped(IslandTypeId islandId, ChainTypeId type, int level, int holderSize) {
			IslandId = islandId;
			Type = type;
			Level = level;
			HolderSize = holderSize;
		}

		public override string EventDescription =>
			Invariant($"Island {IslandId}, Type {Type}, Level {Level}, HolderSize {HolderSize}");
	}

	[AnalyticsEvent(PlayerEventCodes.MergeItemOpened)]
	public class PlayerMergeItemOpened : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public ChainTypeId Type { get; private set; }
		[MetaMember(3)] public int Level { get; private set; }
		[MetaMember(4)] public int TimeLeft { get; private set; }

		public PlayerMergeItemOpened() { }

		public PlayerMergeItemOpened(IslandTypeId island, ChainTypeId type, int level, MetaDuration timeLeft) {
			Island = island;
			Type = type;
			Level = level;
			TimeLeft = F64.RoundToInt(timeLeft.ToSecondsF64());
		}

		public override string EventDescription =>
			Invariant($"Island {Island}, Type {Type}, Level {Level}, TimeLeft {TimeLeft}");
	}

	[AnalyticsEvent(PlayerEventCodes.BuildingDailyRewardClaimed)]
	public class PlayerBuildingDailyRewardClaimed : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }

		public PlayerBuildingDailyRewardClaimed() { }

		public PlayerBuildingDailyRewardClaimed(IslandTypeId island) {
			Island = island;
		}

		public override string EventDescription => Invariant($"Island {Island}");
	}

	[AnalyticsEvent(PlayerEventCodes.BuildingFragmentClaimed)]
	public class PlayerBuildingFragmentClaimed : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public BuildingState State { get; private set; }
		[MetaMember(3)] public int Level { get; private set; }
		[MetaMember(4)] public ChainTypeId ItemType { get; private set; }

		public PlayerBuildingFragmentClaimed() { }

		public PlayerBuildingFragmentClaimed(
			IslandTypeId island,
			BuildingState state,
			int level,
			ChainTypeId itemType
		) {
			Island = island;
			State = state;
			Level = level;
			ItemType = itemType;
		}

		public override string EventDescription =>
			Invariant($"Island {Island}, State {State}, Level {Level}, ItemType {ItemType}");
	}

	[AnalyticsEvent(PlayerEventCodes.HeroTaskRewardsClaimed)]
	public class PlayerHeroTaskRewardsClaimed : PlayerEventBase {
		[MetaMember(1)] public HeroTypeId Hero { get; private set; }

		public PlayerHeroTaskRewardsClaimed() { }

		public PlayerHeroTaskRewardsClaimed(HeroTypeId hero) {
			Hero = hero;
		}

		public override string EventDescription => Invariant($"Hero {Hero}");
	}

	[AnalyticsEvent(PlayerEventCodes.ItemDiscoveryRewardClaimed)]
	public class PlayerItemDiscoveryRewardClaimed : PlayerEventBase {
		[MetaMember(1)] public ChainTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }

		public PlayerItemDiscoveryRewardClaimed() { }

		public PlayerItemDiscoveryRewardClaimed(ChainTypeId type, int level) {
			Type = type;
			Level = level;
		}

		public override string EventDescription => Invariant($"Type {Type}, Level {Level}");
	}

	[AnalyticsEvent(PlayerEventCodes.RewardClaimed)]
	public class PlayerRewardClaimed : PlayerEventBase {
		[MetaMember(1)] public RewardType Type { get; private set; }
		[MetaMember(2)] public int ItemCount { get; private set; }

		public PlayerRewardClaimed() { }

		public PlayerRewardClaimed(RewardType type, int itemCount) {
			Type = type;
			ItemCount = itemCount;
		}

		public override string EventDescription => Invariant($"Type {Type}, ItemCount {ItemCount}");
	}

	[AnalyticsEvent(PlayerEventCodes.HeroUnlocked)]
	public class PlayerHeroUnlocked : PlayerEventBase {
		[MetaMember(1)] public HeroTypeId Hero { get; private set; }
		[MetaMember(2)] public int ItemCount { get; private set; }

		public PlayerHeroUnlocked() { }

		public PlayerHeroUnlocked(HeroTypeId hero, int itemCount) {
			Hero = hero;
			ItemCount = itemCount;
		}

		public override string EventDescription => Invariant($"Hero {Hero}, ItemCount {ItemCount}");
	}

	[AnalyticsEvent(PlayerEventCodes.HeroTaskFulfilled)]
	public class PlayerHeroTaskFulfilled : PlayerEventBase {
		[MetaMember(1)] public HeroTypeId Hero { get; private set; }
		[MetaMember(2)] public int TaskId { get; private set; }

		public PlayerHeroTaskFulfilled() { }

		public PlayerHeroTaskFulfilled(HeroTypeId hero, int taskId) {
			Hero = hero;
			TaskId = taskId;
		}

		public override string EventDescription => Invariant($"Hero {Hero}, TaskId {TaskId}");
	}

	[AnalyticsEvent(PlayerEventCodes.IslandTaskFulfilled)]
	public class PlayerIslandTaskFulfilled : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public IslanderId Islander { get; private set; }
		[MetaMember(3)] public int TaskId { get; private set; }

		public PlayerIslandTaskFulfilled() { }

		public PlayerIslandTaskFulfilled(IslandTypeId island, IslanderId islander, int taskId) {
			Island = island;
			Islander = islander;
			TaskId = taskId;
		}

		public override string EventDescription => Invariant($"Island {Island}, Islander {Islander}, TaskId {TaskId}");
	}

	[AnalyticsEvent(PlayerEventCodes.BoosterItemUsed)]
	public class PlayerBoosterItemUsed : PlayerEventBase {
		[MetaMember(1)] public BoosterTypeId Type { get; private set; }
		[MetaMember(2)] public ChainTypeId TargetType { get; private set; }
		[MetaMember(3)] public int TargetLevel { get; private set; }

		public PlayerBoosterItemUsed() { }

		public PlayerBoosterItemUsed(BoosterTypeId type, ChainTypeId targetType, int targetLevel) {
			Type = type;
			TargetType = targetType;
			TargetLevel = targetLevel;
		}

		public override string EventDescription =>
			Invariant($"Type {Type}, TargetType {TargetType}, TargetLevel {TargetLevel}");
	}

	[AnalyticsEvent(PlayerEventCodes.TriggerExecuted)]
	public class PlayerTriggerExecuted : PlayerEventBase {
		[MetaMember(1)] public TriggerId Trigger { get; private set; }

		public PlayerTriggerExecuted() { }

		public PlayerTriggerExecuted(TriggerId trigger) {
			Trigger = trigger;
		}

		public override string EventDescription => Invariant($"Trigger {Trigger}");
	}

	[AnalyticsEvent(PlayerEventCodes.StoredToBackpack)]
	public class PlayerStoredToBackpack : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public ChainTypeId Type { get; private set; }
		[MetaMember(3)] public int Level { get; private set; }

		public PlayerStoredToBackpack() { }

		public PlayerStoredToBackpack(IslandTypeId island, ChainTypeId type, int level) {
			Island = island;
			Type = type;
			Level = level;
		}

		public override string EventDescription => Invariant($"Island {Island}, Type {Type}, Level {Level}");
	}

	[AnalyticsEvent(PlayerEventCodes.PoppedFromBackpack)]
	public class PlayerPoppedFromBackpack : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public ChainTypeId Type { get; private set; }
		[MetaMember(3)] public int Level { get; private set; }

		public PlayerPoppedFromBackpack() { }

		public PlayerPoppedFromBackpack(IslandTypeId island, ChainTypeId type, int level) {
			Island = island;
			Type = type;
			Level = level;
		}

		public override string EventDescription => Invariant($"Island {Island}, Type {Type}, Level {Level}");
	}

	[AnalyticsEvent(PlayerEventCodes.IslandRemoved)]
	public class PlayerIslandRemoved : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }

		public PlayerIslandRemoved() { }

		public PlayerIslandRemoved(IslandTypeId island) {
			Island = island;
		}

		public override string EventDescription => Invariant($"Island {Island}");
	}

	[AnalyticsEvent(PlayerEventCodes.ItemDiscovered)]
	public class PlayerItemDiscovered : PlayerEventBase {
		[MetaMember(1)] public ChainTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }

		public PlayerItemDiscovered() { }

		public PlayerItemDiscovered(ChainTypeId type, int level) {
			Type = type;
			Level = level;
		}

		public override string EventDescription => Invariant($"Type {Type}, Level {Level}");
	}

	[AnalyticsEvent(PlayerEventCodes.IslandEntered)]
	public class PlayerIslandEntered : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }

		public PlayerIslandEntered() { }

		public PlayerIslandEntered(IslandTypeId island) {
			Island = island;
		}

		public override string EventDescription => Invariant($"Island {Island}");
	}

	[AnalyticsEvent(PlayerEventCodes.MapEntered)]
	public class PlayerMapEntered : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId FromIsland { get; private set; }

		public PlayerMapEntered() { }

		public PlayerMapEntered(IslandTypeId fromIsland) {
			FromIsland = fromIsland;
		}

		public override string EventDescription => Invariant($"FromIsland {FromIsland}");
	}

	[AnalyticsEvent(PlayerEventCodes.BuilderUsed)]
	public class PlayerBuilderUsed : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public ChainTypeId Type { get; private set; }
		[MetaMember(3)] public int Level { get; private set; }
		[MetaMember(4)] public BuilderActionId Action { get; private set; }
		[MetaMember(5)] public int TimeLeft { get; private set; }

		public PlayerBuilderUsed() { }

		public PlayerBuilderUsed(IslandTypeId island, ChainTypeId type, int level, BuilderActionId action, int timeLeft) {
			Island = island;
			Type = type;
			Level = level;
			Action = action;
			TimeLeft = timeLeft;
		}

		public override string EventDescription =>
			Invariant($"Island {Island}, Type {Type}, Level {Level}, Action {Action}, TimeLeft {TimeLeft}");
	}

	[AnalyticsEvent(PlayerEventCodes.MinedItemsClaimed)]
	public class PlayerMinedItemsClaimed : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public ChainTypeId Type { get; private set; }
		[MetaMember(3)] public int Level { get; private set; }
		[MetaMember(4)] public int Count { get; private set; }
		[MetaMember(5)] public int Remaining { get; private set; }

		public PlayerMinedItemsClaimed() { }

		public PlayerMinedItemsClaimed(IslandTypeId island, ChainTypeId type, int level, int count, int remaining) {
			Island = island;
			Type = type;
			Level = level;
			Count = count;
			Remaining = remaining;
		}

		public override string EventDescription =>
			Invariant($"Island {Island}, Type {Type}, Level {Level}, Count {Count}, Remaining {Remaining}");
	}

	[AnalyticsEvent(PlayerEventCodes.EventRewardsClaimed)]
	public class PlayerEventRewardsClaimed : PlayerEventBase {
		[MetaMember(1)] public string EventId { get; private set; }
		[MetaMember(2)] public string Type { get; private set; }
		[MetaMember(3)] public bool HasPremiumPass { get; private set; }
		[MetaMember(4)] public int RewardsClaimedBefore { get; private set; }
		[MetaMember(5)] public int RewardsClaimed { get; private set; }
		[MetaMember(6)] public bool AutoClaim { get; private set; }
		[MetaMember(7)] public MetaTime EventStartTime { get; private set; }

		public PlayerEventRewardsClaimed() { }

		public PlayerEventRewardsClaimed(
			string eventId,
			string type,
			bool hasPremiumPass,
			int rewardsClaimedBefore,
			int rewardsClaimed,
			bool autoClaim,
			MetaTime eventStartTime
		) {
			EventId = eventId;
			Type = type;
			HasPremiumPass = hasPremiumPass;
			RewardsClaimedBefore = rewardsClaimedBefore;
			RewardsClaimed = rewardsClaimed;
			AutoClaim = autoClaim;
			EventStartTime = eventStartTime;
		}

		public override string EventDescription =>
			Invariant(
				$"Type {Type}, EventId {EventId}, HasPremiumPass {HasPremiumPass}, RewardsClaimedBefore {RewardsClaimedBefore}, RewardsClaimed {RewardsClaimed}, AutoClaim {AutoClaim}, EventStartTime: {EventStartTime}"
			);
	}

	[AnalyticsEvent(PlayerEventCodes.EventAdSeen)]
	public class PlayerEventAdSeen : PlayerEventBase {
		[MetaMember(1)] public string EventId { get; private set; }

		public PlayerEventAdSeen() { }

		public PlayerEventAdSeen(string eventId) {
			EventId = eventId;
		}

		public override string EventDescription => Invariant($"EventId {EventId}");
	}

	[AnalyticsEvent(PlayerEventCodes.IslandRevealed)]
	public class PlayerIslandRevealed : PlayerEventBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }

		public PlayerIslandRevealed() { }

		public PlayerIslandRevealed(IslandTypeId island) {
			Island = island;
		}

		public override string EventDescription => Invariant($"Island {Island}");
	}

	[AnalyticsEvent(PlayerEventCodes.LogbookTaskRewardClaimed)]
	public class PlayerLogbookTaskRewardClaimed : PlayerEventBase {
		[MetaMember(1)] public LogbookTaskId Task { get; private set; }

		public PlayerLogbookTaskRewardClaimed() { }

		public PlayerLogbookTaskRewardClaimed(LogbookTaskId task) {
			Task = task;
		}

		public override string EventDescription => Invariant($"Task {Task}");
	}

	[AnalyticsEvent(PlayerEventCodes.LogbookChapterRewardClaimed)]
	public class PlayerLogbookChapterRewardClaimed : PlayerEventBase {
		[MetaMember(1)] public LogbookChapterId Chapter { get; private set; }

		public PlayerLogbookChapterRewardClaimed() { }

		public PlayerLogbookChapterRewardClaimed(LogbookChapterId chapter) {
			Chapter = chapter;
		}

		public override string EventDescription => Invariant($"Chapter {Chapter}");
	}

	[AnalyticsEvent(PlayerEventCodes.AdminRewardCurrency, canTrigger: true)]
	public class PlayerEventAdminRewardCurrency : PlayerEventBase
	{
		[MetaMember(1)] public int? NewGold { get; private set; }
		[MetaMember(2)] public int? NewGems { get; private set; }

		public override string EventDescription
		{
			get
			{
				List<string> changes = new List<string>();
				changes.Add(Invariant($"Player currency rewarded. "));

				if (NewGold.HasValue)
					changes.Add(Invariant($"Gold += {NewGold.Value}, "));
				else
					changes.Add(Invariant($"Gold unchanged, "));

				if (NewGems.HasValue)
					changes.Add(Invariant($"Gems += {NewGems.Value}."));
				else
					changes.Add(Invariant($"Gems unchanged."));

				return string.Join("", changes);
			}
		}

		PlayerEventAdminRewardCurrency() { }
		public PlayerEventAdminRewardCurrency(int? newGold, int? newGems)
		{
			NewGold = newGold;
			NewGems = newGems;
		}
	}
}
