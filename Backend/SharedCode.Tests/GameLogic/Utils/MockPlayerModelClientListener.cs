using System;
using System.Collections.Generic;
using Game.Logic;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Math;

namespace CloudCore.Tests.GameLogic.Utils {
	public class MockPlayerModelClientListener : IPlayerModelClientListener {
		public bool IsDebugOn = false;

		public List<OnItemMovedOnBoardArgs> OnItemMovedOnBoardCalls { get; } = new();
		public List<OnItemCreatedOnBoardArgs> OnItemCreatedOnBoardCalls { get; } = new();
		public List<OnItemRemovedFromBoardArgs> OnItemRemovedFromBoardCalls { get; } = new();
		public int OnRewardClaimedCallCount { get; private set; } = 0;
		public List<OnResourcesModifiedArgs> OnResourcesModifiedCalls { get; } = new();
		public List<OnItemDiscoveryChangedArgs> OnItemDiscoveryChangedCalls { get; } = new();
		public List<OnItemTransferredToIslandArgs> OnItemTransferredToIslandCalls { get; } = new();
		public int OnRewardAddedCallCount { get; private set; } = 0;
		public List<OnActivityEventPremiumPassBoughtArgs> OnActivityEventPremiumPassBoughtCalls { get; } = new();
		public List<OnActivityEventScoreAddedArgs> OnActivityEventScoreAddedCalls { get; } = new();
		public List<OnDailyTaskProgressMadeArgs> OnDailyTaskProgressMadeCalls { get; } = new();
		public List<OnEventStateChangedArgs> OnEventStateChangedCalls { get; } = new();
		public List<OnClaimedInAppProductArgs> OnClaimedInAppProductCalls { get; } = new();
		public int OnVipPassesChangedCallCount { get; private set; } = 0;
		public List<OnLogbookTaskModifiedArgs> OnLogbookTaskModifiedCalls { get; } = new();
		public List<OnLogbookChapterUnlockedArgs> OnLogbookChapterUnlockedCalls { get; } = new();
		public List<OnLogbookChapterModifiedArgs> OnLogbookChapterModifiedCalls { get; } = new();

		public void OnItemMovedOnBoard(
			IslandTypeId islandId,
			ItemModel item,
			int fromX,
			int fromY,
			int toX,
			int toY
		) {
			Debug($"Called {nameof(OnItemMovedOnBoard)}");
			OnItemMovedOnBoardCalls.Add(new OnItemMovedOnBoardArgs(islandId, item, fromX, fromY, toX, toY));
		}

		public void OnItemCreatedOnBoard(
			IslandTypeId islandId,
			ItemModel item,
			int fromX,
			int fromY,
			int toX,
			int toY,
			bool spawned
		) {
			Debug($"Called {nameof(OnItemCreatedOnBoard)}");
			OnItemCreatedOnBoardCalls.Add(new OnItemCreatedOnBoardArgs(islandId, item, fromX, fromY, toX, toY, spawned));
		}

		public void OnItemRemovedFromBoard(IslandTypeId islandId, ItemModel item, int x, int y) {
			Debug($"Called {nameof(OnItemRemovedFromBoard)}");
			OnItemRemovedFromBoardCalls.Add(new OnItemRemovedFromBoardArgs(islandId, item, x, y));
		}

		public void OnItemMerged(IslandTypeId island, ItemModel newItem) {
			Debug($"Called {nameof(OnItemMerged)}");
		}

		public void OnMergeItemStateChanged(IslandTypeId islandId, ItemModel item) {
			Debug($"Called {nameof(OnMergeItemStateChanged)}");
		}

		public void OnResourcesModified(
			CurrencyTypeId resourceType,
			int diff,
			ResourceModificationContext context
		) {
			Debug($"Called {nameof(OnResourcesModified)}");
			OnResourcesModifiedCalls.Add(new OnResourcesModifiedArgs(resourceType, diff, context));
		}

		public void OnHeroUnlocked(HeroTypeId heroType) {
			Debug($"Called {nameof(OnHeroUnlocked)}");
		}

		public void OnNewHeroStarted(HeroTypeId heroType) {
			Debug($"Called {nameof(OnNewHeroStarted)}");
		}

		public void OnHeroTaskModified(HeroTypeId heroType) {
			Debug($"Called {nameof(OnHeroTaskModified)}");
		}

		public void OnIslandTaskModified(IslandTypeId island, IslanderId islander) {
			Debug($"Called {nameof(OnIslandTaskModified)}");
		}

		public void OnItemHolderModified(IslandTypeId island) {
			Debug($"Called {nameof(OnItemHolderModified)}");
		}

		public void OnIslandStateModified(IslandTypeId island) {
			Debug($"Called {nameof(OnIslandStateModified)}");
		}

		public void OnPlayerXpAdded(int delta) {
			Debug($"Called {nameof(OnPlayerXpAdded)}");
		}

		public void OnPlayerLevelUp(RewardModel rewards) {
			Debug($"Called {nameof(OnPlayerLevelUp)}");
		}

		public void OnIslandXpAdded(IslandTypeId island, int delta) {
			Debug($"Called {nameof(OnIslandXpAdded)}");
		}

		public void OnIslandLevelUp(IslandTypeId island, RewardModel rewards) {
			Debug($"Called {nameof(OnIslandLevelUp)}");
		}

		public void OnBuildingXpAdded(IslandTypeId island, int delta) {
			Debug($"Called {nameof(OnBuildingXpAdded)}");
		}

		public void OnBuildingLevelUp(IslandTypeId island, RewardModel rewards) {
			Debug($"Called {nameof(OnBuildingLevelUp)}");
		}

		public void OnHeroXpAdded(HeroTypeId hero, int delta) {
			Debug($"Called {nameof(OnHeroXpAdded)}");
		}

		public void OnHeroLevelUp(HeroTypeId hero, RewardModel rewards) {
			Debug($"Called {nameof(OnHeroLevelUp)}");
		}

		public void OnBuildingFragmentCollected(IslandTypeId island, ItemModel item, int x, int y) {
			Debug($"Called {nameof(OnBuildingFragmentCollected)}");
		}

		public void OnBuildingRevealed(IslandTypeId island) {
			Debug($"Called {nameof(OnBuildingRevealed)}");
		}

		public void OnBuildingCompleted(IslandTypeId island) {
			Debug($"Called {nameof(OnBuildingCompleted)}");
		}

		public void OnItemTransferredToIsland(IslandTypeId island, ItemModel item, int x, int y) {
			Debug($"Called {nameof(OnItemTransferredToIsland)}");
			OnItemTransferredToIslandCalls.Add(new OnItemTransferredToIslandArgs(island, item, x, y));
		}

		public void OnRewardAdded() {
			Debug($"Called {nameof(OnRewardAdded)}");
			OnRewardAddedCallCount++;
		}

		public void OnRewardClaimed() {
			Debug($"Called {nameof(OnRewardClaimed)}");
			OnRewardClaimedCallCount++;
		}

		public void OnItemDiscoveryChanged(LevelId<ChainTypeId> chainId) {
			Debug($"Called {nameof(OnItemDiscoveryChanged)}");
			OnItemDiscoveryChangedCalls.Add(new OnItemDiscoveryChangedArgs(chainId));
		}

		public void OnLockAreaUnlocked(IslandTypeId islandId, char areaIndex) {
			Debug($"Called {nameof(OnLockAreaUnlocked)}");
		}

		public void OnLockAreaOpened(IslandTypeId islandId, char areaIndex) {
			Debug($"Called {nameof(OnLockAreaOpened)}");
		}

		public void OnFeatureUnlocked(FeatureTypeId feature) {
			Debug($"Called {nameof(OnFeatureUnlocked)}");
		}

		public void OnDialogueStarted(DialogueId dialogue) {
			Debug($"Called {nameof(OnDialogueStarted)}");
		}

		public void OnHighlightElement(string element) {
			Debug($"Called {nameof(OnHighlightElement)}");
		}

		public void OnHighlightItem(ChainTypeId type, int level) {
			Debug($"Called {nameof(OnHighlightItem)}");
		}

		public void OnPointItem(ChainTypeId type, int level) {
			Debug($"Called {nameof(OnPointItem)}");
		}

		public void OnMergeHint(ChainTypeId type1, int level1, ChainTypeId type2, int level2) {
			Debug($"Called {nameof(OnMergeHint)}");
		}

		public void OnShopUpdated() {
			Debug($"Called {nameof(OnShopUpdated)}");
		}

		public void OnMarketItemUpdated(LevelId<ShopCategoryId> shopItemId) {
			Debug($"Called {nameof(OnMarketItemUpdated)}");
		}

		public void OnBackpackUpgraded() {
			Debug($"Called {nameof(OnBackpackUpgraded)}");
		}

		public void OnItemStoredToBackpack(IslandTypeId island, ItemModel item, int x, int y) {
			Debug($"Called {nameof(OnItemStoredToBackpack)}");
		}

		public void OnItemRemovedFromBackpack(int index, ItemModel item) {
			Debug($"Called {nameof(OnItemRemovedFromBackpack)}");
		}

		public void OnIslandRemoved(IslandTypeId island) {
			Debug($"Called {nameof(OnIslandRemoved)}");
		}

		public void OnEnterIsland(IslandTypeId triggerEnterIsland) {
			Debug($"Called {nameof(OnEnterIsland)}");
		}

		public void OnGoToIsland(IslandTypeId island) {
			Debug($"Called {nameof(OnGoToIsland)}");
		}

		public void OnHighlightIsland(IslandTypeId island) {
			Debug($"Called {nameof(OnHighlightIsland)}");
		}

		public void OnPointIsland(IslandTypeId island) {
			Debug($"Called {nameof(OnPointIsland)}");
		}

		public void OnClaimedInAppProduct(InAppProductInfo product, F64 referencePrice) {
			Debug($"Called {nameof(OnClaimedInAppProduct)}");
			OnClaimedInAppProductCalls.Add(new OnClaimedInAppProductArgs(product, referencePrice));
		}

		public void OnBuilderStateChanged() {
			Debug($"Called {nameof(OnBuilderStateChanged)}");
		}

		public void OnBuilderFinished(ItemModel item) {
			Debug($"Called {nameof(OnBuilderFinished)}");
		}

		public void OnActivityEventScoreAdded(EventId eventId, int level, int delta, ResourceModificationContext context) {
			Debug($"Called {nameof(OnActivityEventScoreAdded)}");
			OnActivityEventScoreAddedCalls.Add(new OnActivityEventScoreAddedArgs(eventId, level, delta, context));
		}

		public void OnActivityEventLevelUp(EventId eventId, RewardModel rewards) {
			Debug($"Called {nameof(OnActivityEventLevelUp)}");
		}

		public void OnActivityEventPremiumPassBought(EventId eventId) {
			Debug($"Called {nameof(OnActivityEventPremiumPassBought)}");
			OnActivityEventPremiumPassBoughtCalls.Add(new OnActivityEventPremiumPassBoughtArgs(eventId));
		}

		public void OnHeroMovedToBuilding(HeroTypeId hero, ChainTypeId sourceBuilding, ChainTypeId targetBuilding) {
			Debug($"Called {nameof(OnHeroMovedToBuilding)}");
		}

		public void OnEventStateChanged(EventId eventId) {
			Debug($"Called {nameof(OnEventStateChanged)}");
			OnEventStateChangedCalls.Add(new OnEventStateChangedArgs(eventId));
		}

		public void OnDailyTaskProgressMade(EventId eventId, int progressAmount, ResourceModificationContext context) {
			Debug($"Called {nameof(OnDailyTaskProgressMade)}");
			OnDailyTaskProgressMadeCalls.Add(new OnDailyTaskProgressMadeArgs(eventId, progressAmount, context));
		}

		public void OnActivityEventRewardClaimed(EventId eventId, int level, bool premium) {
			Debug($"Called {nameof(OnActivityEventRewardClaimed)}");
		}

		public void OnBuilderUsed(IslandTypeId island, ItemModel item, int duration) {
			Debug($"Called {nameof(OnBuilderUsed)}");
		}

		public void OnOpenOffer(InAppProductId product) {
			Debug($"Called {nameof(OnOpenOffer)}");
		}

		public void OnVipPassesChanged() {
			Debug($"Called {nameof(OnVipPassesChanged)}");
			OnVipPassesChangedCallCount++;
		}

		public void OnLogbookTaskModified(LogbookTaskId id) {
			Debug($"Called {nameof(OnLogbookTaskModified)}");
			OnLogbookTaskModifiedCalls.Add(new OnLogbookTaskModifiedArgs(id));
		}

		public void OnLogbookChapterUnlocked(LogbookChapterId id) {
			Debug($"Called {nameof(OnLogbookChapterUnlocked)}");
			OnLogbookChapterUnlockedCalls.Add(new OnLogbookChapterUnlockedArgs(id));
		}

		public void OnLogbookChapterModified(LogbookChapterId id) {
			Debug($"Called {nameof(OnLogbookChapterModified)}");
			OnLogbookChapterModifiedCalls.Add(new OnLogbookChapterModifiedArgs(id));
		}

		public void OnOpenInfo(string url) {
			Debug($"Called {nameof(OnOpenInfo)}");
		}

		public void OnMergeScoreChanged(int mergeScore) {
		}

		private void Debug(object msg) {
			if (IsDebugOn) {
				Console.Out.WriteLine(msg);
			}
		}
	}

	public class OnItemMovedOnBoardArgs {
		public IslandTypeId IslandId { get; }
		public ItemModel Item { get; }
		public int FromX { get; }
		public int FromY { get; }
		public int ToX { get; }
		public int ToY { get; }

		public OnItemMovedOnBoardArgs(
			IslandTypeId islandId,
			ItemModel item,
			int fromX,
			int fromY,
			int toX,
			int toY
		) {
			IslandId = islandId;
			Item = item;
			FromX = fromX;
			FromY = fromY;
			ToX = toX;
			ToY = toY;
		}
	}

	public struct OnItemCreatedOnBoardArgs {
		public IslandTypeId IslandId { get; }
		public ItemModel Item { get; }
		public int FromX { get; }
		public int FromY { get; }
		public int ToX { get; }
		public int ToY { get; }
		public bool Spawned { get; }

		public OnItemCreatedOnBoardArgs(
			IslandTypeId islandId,
			ItemModel item,
			int fromX,
			int fromY,
			int toX,
			int toY,
			bool spawned
		) {
			IslandId = islandId;
			Item = item;
			FromX = fromX;
			FromY = fromY;
			ToX = toX;
			ToY = toY;
			Spawned = spawned;
		}
	}

	public struct OnItemRemovedFromBoardArgs {
		public bool Equals(OnItemRemovedFromBoardArgs other) {
			return Equals(IslandId, other.IslandId) && Equals(Item, other.Item) && X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj) {
			return obj is OnItemRemovedFromBoardArgs other && Equals(other);
		}

		public override int GetHashCode() {
			return HashCode.Combine(IslandId, Item, X, Y);
		}

		public IslandTypeId IslandId { get; }
		public ItemModel Item { get; }
		public int X { get; }
		public int Y { get; }

		public OnItemRemovedFromBoardArgs(IslandTypeId islandId, ItemModel item, int x, int y) {
			IslandId = islandId;
			Item = item;
			X = x;
			Y = y;
		}
	}

	public struct OnResourcesModifiedArgs {
		public CurrencyTypeId ResourceType { get; }
		public int Diff { get; }
		public ResourceModificationContext Context { get; }

		public OnResourcesModifiedArgs(CurrencyTypeId resourceType, int diff, ResourceModificationContext context) {
			ResourceType = resourceType;
			Diff = diff;
			Context = context;
		}
	}

	public struct OnItemDiscoveryChangedArgs {
		public LevelId<ChainTypeId> ChainId { get; }

		public OnItemDiscoveryChangedArgs(LevelId<ChainTypeId> chainId) {
			ChainId = chainId;
		}

		public override string ToString() {
			return $"{nameof(OnItemDiscoveryChangedArgs)}, ChainId:${ChainId}";
		}
	}

	public struct OnItemTransferredToIslandArgs {
		public IslandTypeId Island { get; }
		public ItemModel Item { get; }
		public int X { get; }
		public int Y { get; }

		public OnItemTransferredToIslandArgs(IslandTypeId island, ItemModel item, int x, int y) {
			Island = island;
			Item = item;
			X = x;
			Y = y;
		}
	}

	public struct OnActivityEventPremiumPassBoughtArgs {
		public EventId EventId { get; }

		public OnActivityEventPremiumPassBoughtArgs(EventId eventId) {
			EventId = eventId;
		}
	}

	public struct OnActivityEventScoreAddedArgs {
		public EventId EventId { get; }
		public int Level { get; }
		public int Delta { get; }
		public ResourceModificationContext Context { get; }

		public OnActivityEventScoreAddedArgs(EventId eventId, int level, int delta, ResourceModificationContext context) {
			EventId = eventId;
			Level = level;
			Delta = delta;
			Context = context;
		}
	}

	public struct OnDailyTaskProgressMadeArgs {
		public EventId EventId { get; }
		public int ProgressAmount { get; }
		public ResourceModificationContext Context { get; }

		public OnDailyTaskProgressMadeArgs(EventId eventId, int progressAmount, ResourceModificationContext context) {
			EventId = eventId;
			ProgressAmount = progressAmount;
			Context = context;
		}
	}

	public struct OnEventStateChangedArgs {
		public EventId EventId { get; }

		public OnEventStateChangedArgs(EventId eventId) {
			EventId = eventId;
		}

		public override string ToString() {
			return $"{EventId}";
		}
	}

	public struct OnClaimedInAppProductArgs {
		public InAppProductInfo Product { get; }
		public F64 ReferencePrice { get; }

		public OnClaimedInAppProductArgs(InAppProductInfo product, F64 referencePrice) {
			Product = product;
			ReferencePrice = referencePrice;
		}
	}

	public struct OnLogbookTaskModifiedArgs {
		public LogbookTaskId Id { get; }

		public OnLogbookTaskModifiedArgs(LogbookTaskId id) {
			Id = id;
		}

		public override string ToString() {
			return $"{nameof(OnLogbookTaskModifiedArgs)}, Id:${Id}";
		}
	}

	public struct OnLogbookChapterUnlockedArgs {
		public LogbookChapterId Id { get; }

		public OnLogbookChapterUnlockedArgs(LogbookChapterId id) {
			Id = id;
		}
	}

	public struct OnLogbookChapterModifiedArgs {
		public LogbookChapterId Id { get; }

		public OnLogbookChapterModifiedArgs(LogbookChapterId id) {
			Id = id;
		}
	}
}
