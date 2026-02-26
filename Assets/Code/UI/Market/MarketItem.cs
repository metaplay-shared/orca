using Code.Purchasing;
using System.Threading;
using Code.UI.Application;
using Code.UI.AssetManagement;
using Code.UI.Shop;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Market {
	public class MarketItem : MonoBehaviour {
		[SerializeField] private GameObject LeftLabel;
		[SerializeField] private TMP_Text LeftText;
		[SerializeField] private Image Icon;
		[SerializeField] private TMP_Text AmountText;
		[SerializeField] private TMP_Text ItemLabel;
		[SerializeField] private CurrencyLabel CostLabel;
		[SerializeField] private Button PurchaseButton;

		[Inject] private SignalBus signalBus;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private AddressableManager addressableManager;
		[Inject] private IPurchasingFlowController purchasingFlowController;

		private MarketItemModel item;

		public async UniTask Setup(MarketItemModel item) {
			this.item = item;
			if (item.Info.ItemType == ChainTypeId.None) {
				LeftLabel.SetActive(false);
				PurchaseButton.interactable = true;
				Icon.sprite = await addressableManager.GetLazy<Sprite>($"Offers/{item.Info.Icon}.png");
				ItemLabel.text = Localizer.Localize($"Market.Currencies.{item.Info.Icon}");
			} else {
				LeftText.text = Localizer.Localize("Market.ItemsLeft", item.ItemsLeft);
				LeftLabel.SetActive(true);
				PurchaseButton.interactable = item.ItemsLeft > 0;
				ChainTypeId type = MetaplayClient.PlayerModel.MapRewardToRealTypeUI(applicationInfo.ActiveIsland.Value, item.Info.ItemType);
				ChainInfo chainInfo =
					MetaplayClient.PlayerModel.GameConfig.Chains[new LevelId<ChainTypeId>(type, item.Info.ItemLevel)];
				Icon.sprite = addressableManager.GetItemIcon(chainInfo);
				ItemLabel.text = Localizer.Localize($"Chain.{type}.{item.Info.ItemLevel}");
			}
			AmountText.text = "x" + item.Info.Count;
			CostLabel.Set(item.Info.CostType, item.Info.Cost);
		}

		private void OnEnable() {
			signalBus.Subscribe<MarketItemUpdatedSignal>(OnMarketItemUpdated);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<MarketItemUpdatedSignal>(OnMarketItemUpdated);
		}

		public void OnPurchaseClicked() {
			if (item.Info.CostType == CurrencyTypeId.Gems) {
				purchasingFlowController.TrySpendGemsAsync(item.Info.Cost, CancellationToken.None)
					.ContinueWith(
						success => {
							if (success) {
								Buy();
							}
						}
					).Forget();
			} else {
				if (MetaplayClient.PlayerModel.Wallet.EnoughCurrency(item.Info.CostType, item.Info.Cost)) {
					Buy();
				} else {
					signalBus.Fire(new OpenMarketCategorySignal(ShopCategoryId.Gold));
				}
			}

			void Buy() {
				MetaplayClient.PlayerContext.ExecuteAction(
					new PlayerPurchaseMarketItem(
						item.Info.Category,
						item.Info.Index,
						applicationInfo.ActiveIsland.Value
					)
				);
			}
		}

		public void OnInfoClicked() {
			// TODO: Add info popup for items
		}

		private void OnMarketItemUpdated(MarketItemUpdatedSignal signal) {
			if (signal.ItemId.Equals(item.Info.ConfigKey)) {
				LeftText.text = Localizer.Localize("Market.ItemsLeft", item.ItemsLeft);
				PurchaseButton.interactable = item.ItemsLeft > 0;
			}
		}
	}
}
