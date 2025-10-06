using Code.DailyTasks;
using Code.Inbox;
using Code.Logbook;
using System.Linq;
using System.Threading;
using Code.UI;
using Code.UI.Application;
using Code.UI.Application.Signals;
using Code.UI.Core;
using Code.UI.Deletion;
using Code.UI.Effects;
using Code.UI.Events;
using Code.UI.Hud.Signals;
using Code.UI.ItemDiscovery.Signals;
using Code.UI.ItemHolder.Signals;
using Code.UI.Map.Signals;
using Code.UI.Market;
using Code.UI.Merge;
using Code.UI.Merge.AddOns.MergeBoard.LockArea;
using Code.UI.Merge.Hero;
using Code.UI.MergeBase;
using Code.UI.MergeBase.Signals;
using Code.UI.Rewarding;
using Code.UI.Shop;
using Code.UI.Tasks.Hero;
using Code.UI.Tasks.Signals;
using Code.UI.Tutorial;
using Code.UI.Tutorial.TriggerActions;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.InGameMail;
using Metaplay.Core.Math;
using Metaplay.Core.Player;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code {
	public class PlayerModelClientListener : IPlayerModelClientListener, IPlayerModelClientListenerCore {
		[Inject] private SignalBus signalBus;
		[Inject] private MergeBoardRoot mergeBoard;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private TriggerQueue triggerQueue;
		[Inject] private ItemCreatedSignalHandler itemCreatedSignalHandler;
		[Inject] private EffectsController effectsController;
		[Inject] private DiContainer container;
		[Inject] private IUIRootController uiRootController;
		[Inject] private ILogbookTasksController logbookTasksController;
		[Inject] private IItemDiscoveryController itemDiscoveryController;
		[Inject] private IInboxController inboxController;
		[Inject] private IDailyTasksController dailyTasksController;
		[Inject] private IDeletionController deletionController;

		void IPlayerModelClientListenerCore.OnPlayerNameChanged(string newName) { }

		void IPlayerModelClientListenerCore.PendingDynamicPurchaseContentAssigned(InAppProductId productId) { }

		void IPlayerModelClientListenerCore.PendingStaticInAppPurchaseContextAssigned(InAppProductId productId) { }

		void IPlayerModelClientListenerCore.InAppPurchaseValidationFailed(InAppPurchaseEvent ev) { }

		void IPlayerModelClientListenerCore.InAppPurchaseValidated(InAppPurchaseEvent ev) { }

		void IPlayerModelClientListenerCore.InAppPurchaseClaimed(InAppPurchaseEvent ev) { }

		void IPlayerModelClientListenerCore.DuplicateInAppPurchaseCleared(InAppPurchaseEvent ev) { }

		void IPlayerModelClientListenerCore.OnPlayerScheduledForDeletionChanged() {
			deletionController.ScheduledForDeletionChanged();
		}

		void IPlayerModelClientListenerCore.OnNewInGameMail(PlayerMailItem mail) {
			inboxController.NotifyNewInGameMail(mail);
		}

		void IPlayerModelClientListenerCore.OnMailStateChanged(PlayerMailItem mail) {
			inboxController.NotifyMailStateChanged(mail);
		}

		public void OnItemMovedOnBoard(IslandTypeId islandId, ItemModel item, int fromX, int fromY, int toX, int toY) {
			signalBus.Fire(new ItemMovedSignal(islandId.Value, item, fromX, fromY, toX, toY));
			signalBus.Fire(new ItemsOnBoardChangedSignal());
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
			MergeItemModelAdapter adapter = new MergeItemModelAdapter(item, islandId);
			container.Inject(adapter);

			bool fromItemHolder = fromX == StaticConfig.ItemHolderX && fromY == StaticConfig.ItemHolderY;

			itemCreatedSignalHandler.Enqueue(new ItemCreatedSignal(islandId, adapter, fromX, fromY, fromItemHolder, spawned));

			//signalBus.Fire(new ItemsOnBoardChangedSignal());
		}

		public void OnItemRemovedFromBoard(IslandTypeId islandId, ItemModel item, int x, int y) {
			signalBus.Fire(new ItemRemovedSignal(islandId.Value, x, y));
			signalBus.Fire(new ItemsOnBoardChangedSignal());
		}

		public void OnItemMerged(IslandTypeId island, ItemModel newItem) {
			signalBus.Fire(new ItemMergedSignal(island, newItem));
		}

		public void OnMergeItemStateChanged(IslandTypeId islandId, ItemModel item) {
			signalBus.Fire(new ItemStateChangedSignal(islandId, item));
			signalBus.Fire(new ItemsOnBoardChangedSignal());
		}

		public void OnResourcesModified(CurrencyTypeId resourceType, int diff, ResourceModificationContext context) {
			if (context is MergeBoardResourceContext mergeBoardResourceContext) {
				effectsController.FlyCurrencyParticles(
					resourceType,
					diff,
					mergeBoardResourceContext.X,
					mergeBoardResourceContext.Y
				).Forget();
			} else {
				signalBus.Fire(new ResourcesChangedSignal(resourceType, diff));
			}
		}

		public void OnHeroUnlocked(HeroTypeId heroType) {
			signalBus.Fire(new HeroUnlockedSignal(heroType));
			//triggerQueue.EnqueueAction(new HeroUnlockedTriggerAction(heroType));
		}

		public void OnNewHeroStarted(HeroTypeId heroType) {
			//throw new System.NotImplementedException();
		}

		public void OnHeroTaskModified(HeroTypeId heroType) {
			signalBus.Fire(new HeroTaskModifiedSignal(heroType));
		}

		public void OnIslandTaskModified(IslandTypeId island, IslanderId islander) {
			signalBus.Fire(new IslandTaskModifiedSignal(island, islander));
		}

		public void OnItemHolderModified(IslandTypeId island) {
			signalBus.Fire(new ItemHolderModifiedSignal());
		}

		public void OnIslandStateModified(IslandTypeId island) {
			signalBus.Fire(new IslandStateChangedSignal(island));
		}

		public void OnPlayerXpAdded(int delta) {
			signalBus.Fire(new PlayerXpChangedSignal());
			if ((MetaplayClient.PlayerModel.DivisionClientState?.CurrentDivision.IsValid ?? true))
				return;

			MetaplayClient.LeagueClient?.TryJoinLeagues();
		}

		public void OnPlayerLevelUp(RewardModel rewards) {
			signalBus.Fire(new PlayerLevelChangedSignal());
		}

		public void OnIslandXpAdded(IslandTypeId island, int delta) { }
		public void OnIslandLevelUp(IslandTypeId island, RewardModel rewards) { }
		public void OnBuildingXpAdded(IslandTypeId island, int delta) { }
		public void OnBuildingLevelUp(IslandTypeId island, RewardModel rewards) { }
		public void OnHeroXpAdded(HeroTypeId hero, int delta) { }
		public void OnHeroLevelUp(HeroTypeId hero, RewardModel rewards) { }

		public void OnBuildingFragmentCollected(IslandTypeId island, ItemModel item, int x, int y) {
			signalBus.Fire(new BuildingChangedSignal(island));
		}

		public void OnBuildingRevealed(IslandTypeId island) {
			Debug.Log("Building revealed");
		}

		public void OnBuildingCompleted(IslandTypeId island) {
			signalBus.Fire(new MergeBoardStateChangedSignal(island));
		}

		public void OnItemTransferredToIsland(IslandTypeId island, ItemModel item, int x, int y) { }

		public void OnRewardAdded() {
			triggerQueue.EnqueueAction(new RewardTriggerAction());
		}

		public void OnRewardClaimed() {
			signalBus.Fire(new RewardClaimedSignal());
		}

		public void OnItemDiscoveryChanged(LevelId<ChainTypeId> chainId) {
			itemDiscoveryController.OnItemDiscoveryChanged(chainId);
			signalBus.Fire(new ItemDiscoveryStateChangedSignal(chainId));
		}

		public void OnLockAreaUnlocked(IslandTypeId islandId, char areaIndex) {
			signalBus.Fire(new AreaStateChangedSignal(islandId, areaIndex.ToString(), AreaState.Opening));
		}

		public void OnLockAreaOpened(IslandTypeId islandId, char areaIndex) {
			signalBus.Fire(new AreaStateChangedSignal(islandId, areaIndex.ToString(), AreaState.Open));
		}

		public void OnFeatureUnlocked(FeatureTypeId feature) {
			signalBus.Fire(new FeatureUnlockedSignal(feature));
		}

		public void OnDialogueStarted(DialogueId dialogue) {
			triggerQueue.EnqueueAction(new DialogueTriggerAction(dialogue));
		}

		public void OnHighlightElement(string element) {
			triggerQueue.EnqueueAction(new HighlightElementTriggerAction(element));
		}

		public void OnHighlightItem(ChainTypeId type, int level) {
			if (applicationInfo.ActiveIsland.Value == IslandTypeId.None) {
				Debug.LogWarning("Player not in any island");
				return;
			}

			var itemModel = MetaplayClient
				.PlayerModel
				.Islands[applicationInfo.ActiveIsland.Value]
				.MergeBoard
				.Items.FirstOrDefault(i => i.Info.Type == type && i.Info.Level == level && i.State == ItemState.Free);

			if (itemModel == null) {
				Debug.LogWarning($"Item {type} {level} not found on board on active island");
				return;
			}

			//signalBus.Fire(new HighlightItemSignal(itemModel));
			triggerQueue.EnqueueAction(new HighlightItemTriggerAction(itemModel));
		}

		public void OnPointItem(ChainTypeId type, int level) {
			if (applicationInfo.ActiveIsland.Value == IslandTypeId.None) {
				Debug.LogWarning("Player not in any island");
				return;
			}

			var board = MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value].MergeBoard;
			var items = board.Items.FindAll(
				i => i.Info.Type == type && i.Info.Level == level && board.LockArea.IsFree(i.X, i.Y) && (i.State == ItemState.Free || i.State == ItemState.FreeForMerge)
			);
			var action = new PointItemsTriggerAction(items);
			container.Inject(action);
			triggerQueue.EnqueueAction(action);
		}

		public void OnMergeHint(ChainTypeId type1, int level1, ChainTypeId type2, int level2) {
			triggerQueue.EnqueueAction(new MergeHintTriggerAction(type1, type2, level1, level2));
		}

		public void OnShopUpdated() {
			signalBus.Fire(new MarketUpdatedSignal());
		}

		public void OnMarketItemUpdated(LevelId<ShopCategoryId> itemId) {
			signalBus.Fire(new MarketItemUpdatedSignal(itemId));
		}

		public void OnBackpackUpgraded() { }

		public void OnItemStoredToBackpack(IslandTypeId island, ItemModel item, int x, int y) { }

		public void OnItemRemovedFromBackpack(int index, ItemModel item) { }

		public void OnIslandRemoved(IslandTypeId island) {
			signalBus.Fire(new IslandRemovedSignal(island));
		}

		public void OnGoToIsland(IslandTypeId island) {
			signalBus.Fire(new IslandFocusedSignal(island));
		}

		public void OnHighlightIsland(IslandTypeId island) {
			//signalBus.Fire(new IslandHighlightedSignal(island));
			triggerQueue.EnqueueAction(new HighlightIslandAction(island));
		}

		public void OnPointIsland(IslandTypeId island) {
			signalBus.Fire(new IslandPointedSignal(island));
		}

		public void OnClaimedInAppProduct(InAppProductInfo product, F64 referencePrice) {
			if (product.Resources.Count > 0 || product.Items.Count > 0) {
				uiRootController.ShowUI<ClaimedProductPopup, ClaimedProductPopupPayload>(
					new ClaimedProductPopupPayload(product),
					CancellationToken.None
				);
			}
		}

		public void OnBuilderStateChanged() {
			signalBus.Fire(new BuildersChangedSignal());
		}

		public void OnBuilderFinished(ItemModel item) { }

		public void OnActivityEventScoreAdded(EventId eventId, int level, int delta, ResourceModificationContext context) {
			signalBus.Fire(new ActivityEventScoreAddedSignal(eventId, level, delta));
			if (context is MergeBoardResourceContext mergeBoardResourceContext) {
				effectsController.FlyScoreParticles(delta / 10, mergeBoardResourceContext.X, mergeBoardResourceContext.Y).Forget();
			}
		}

		public void OnActivityEventLevelUp(EventId eventId, RewardModel rewards) { }

		public void OnActivityEventPremiumPassBought(EventId eventId) {
			signalBus.Fire(new ActivityEventPremiumPassBoughtSignal(eventId));
		}

		public void OnHeroMovedToBuilding(HeroTypeId hero, ChainTypeId sourceBuilding, ChainTypeId targetBuilding) {
			signalBus.Fire(new HeroAssignedToBuildingSignal(hero, sourceBuilding, targetBuilding));
		}

		public void OnDailyTaskProgressMade(EventId eventId, int progressAmount, ResourceModificationContext context) {
			dailyTasksController.NotifyDailyTaskProgressionMade(eventId, progressAmount, context);
		}

		public void OnEventStateChanged(EventId eventId) {
			signalBus.Fire(new EventStateChangedSignal(eventId));
		}

		public void OnActivityEventRewardClaimed(EventId eventId, int level, bool premium) {
			signalBus.Fire(new ActivityEventRewardClaimedSignal(eventId, level, premium));
		}

		public void OnBuilderUsed(IslandTypeId island, ItemModel item, int duration) {
			signalBus.Fire(new BuilderUsedSignal(island, item, duration));
		}

		public void OnOpenOffer(InAppProductId product) {
			triggerQueue.EnqueueAction(new OfferTriggerAction(product));
		}

		public void OnVipPassesChanged() {
			// TODO
		}

		public void OnLogbookTaskModified(LogbookTaskId id) {
			logbookTasksController.OnLogbookTaskModified(id);
		}

		public void OnLogbookChapterUnlocked(LogbookChapterId id) {
			logbookTasksController.OnLogbookChapterUnlocked(id);
		}

		public void OnLogbookChapterModified(LogbookChapterId id) {
			logbookTasksController.OnLogbookChapterModified(id);
		}

		public void OnOpenInfo(string url) {
			#if UNITY_WEBGL && !UNITY_EDITOR
			GameWebGLApiBridge.UpdateInfoUrl(url);
			#endif
		}

		public void OnMergeScoreChanged(int mergeScore)
		{
			signalBus.Fire(new ResourcesChangedSignal(CurrencyTypeId.MergeEvent, mergeScore));
		}
	}
}
