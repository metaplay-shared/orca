using System.Collections.Generic;
using System.Threading;
using Code.Purchasing;
using Code.UI.Application;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Market {
	public class MarketContentPresenter : MonoBehaviour {
		[SerializeField] private ScrollRect ScrollRect;
		[SerializeField] private RectTransform MarketContent;
		[SerializeField] private RectTransform ViewPort;
		[SerializeField] private MarketItem ItemTemplate;
		[SerializeField] private MarketCategory CategoryTemplate;
		[SerializeField] private TMP_Text NewItemsLabel;
		[SerializeField] private CurrencyLabel RefreshLabel;
		[SerializeField] private Button RefreshButton;

		[Inject] protected SignalBus signalBus;
		[Inject] protected ApplicationInfo applicationInfo;
		[Inject] private DiContainer diContainer;
		[Inject] private IPurchasingFlowController purchasingFlowController;

		private readonly Dictionary<ShopCategoryId, MarketCategory> categories = new();

		private void OnDisable() {
			UnsubscribeSignals();
		}

		private void Refresh() {
			Clear();
			SpawnItems();
		}

		private float nextUpdate = 0;

		private void Update() {
			if (Time.time >= nextUpdate) {
				MetaDuration duration = MetaplayClient.PlayerModel.Market.NextRefreshAt -
					MetaplayClient.PlayerModel.CurrentTime;
				string timeStr = duration.ToSimplifiedString();
				NewItemsLabel.text = Localizer.Localize("Market.RefreshLabel", timeStr);
				nextUpdate = Time.time + 1;
			}
		}

		protected void Awake() {
			SubscribeSignals();

			ResourceInfo cost = MetaplayClient.PlayerModel.GameConfig.Shop.RefreshCost;
			RefreshLabel.Set(cost.Type, cost.Amount);
			Clear();
			SpawnItems();

			RefreshButton.OnClickAsObservable().Subscribe(_ => OnRefreshClicked()).AddTo(gameObject);
		}

		private void SubscribeSignals() {
			signalBus.Subscribe<OpenMarketCategorySignal>(OpenCategory);
			signalBus.Subscribe<MarketUpdatedSignal>(Refresh);
		}

		private void UnsubscribeSignals() {
			signalBus.TryUnsubscribe<OpenMarketCategorySignal>(OpenCategory);
			signalBus.TryUnsubscribe<MarketUpdatedSignal>(Refresh);
		}

		private void OpenCategory(OpenMarketCategorySignal signal) {
			ShowCategory(signal.Category, this.GetCancellationTokenOnDestroy()).Forget();
		}

		private void OnRefreshClicked() {
			ResourceInfo cost = MetaplayClient.PlayerModel.GameConfig.Shop.RefreshCost;

			purchasingFlowController.TrySpendGemsAsync(cost.Amount, CancellationToken.None)
				.ContinueWith(
					success => {
						if (success) {
							MetaplayClient.PlayerContext.ExecuteAction(new PlayerRefreshFlashSales());
							Refresh();
						}
					}
				).Forget();
		}

		private void Clear() {
			categories.Clear();
			foreach (Transform child in MarketContent) {
				Destroy(child.gameObject);
			}
		}

		public async UniTask ShowCategory(ShopCategoryId category, CancellationToken ct) {
			if (!categories.ContainsKey(category)) {
				return;
			}

			await UniTask.DelayFrame(3, cancellationToken: ct);
			RectTransform marketCategory = (RectTransform)categories[category].transform;
			float viewPortHeight = ViewPort.rect.height;
			float categoryHeight = marketCategory.rect.height;
			float contentHeight = ScrollRect.content.rect.height;
			float position = marketCategory.anchoredPosition.y - categoryHeight / 2 - (viewPortHeight - categoryHeight);
			float scrollValue = (contentHeight + position) / (contentHeight - viewPortHeight);
			scrollValue = Mathf.Max(0, scrollValue);
			await ScrollRect.DOVerticalNormalizedPos(scrollValue, 0.2f).ToUniTask(cancellationToken: ct);
		}

		private void SpawnItems() {
			var items = MetaplayClient.PlayerModel.Market.GetMarketItems(
				MetaplayClient.PlayerModel,
				applicationInfo.ActiveIsland.Value
			);
			foreach (var category in MetaplayClient.PlayerModel.GameConfig.Shop.CategoryOrder) {
				var flashSaleItems = items.GetValueOrDefault(category);
				if (flashSaleItems != null) {
					var marketCategory = SpawnCategory(category);
					categories[category] = marketCategory;
					foreach (var item in flashSaleItems)
					{
						if (item.Info.Segment == null || item.Info.Segment.Ref.MatchesPlayer(MetaplayClient.PlayerModel))
						{
							SpawnItem(marketCategory.Content.transform, item);	
						}
					}
				}
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.content);
		}

		private MarketCategory SpawnCategory(ShopCategoryId categoryId) {
			var category = diContainer.InstantiatePrefabForComponent<MarketCategory>(CategoryTemplate, MarketContent);
			category.Setup(categoryId);
			return category;
		}

		private void SpawnItem(Transform container, MarketItemModel item) {
			MarketItem marketItem = diContainer.InstantiatePrefabForComponent<MarketItem>(ItemTemplate, container);
			marketItem.Setup(item).Forget();
		}
	}
}
