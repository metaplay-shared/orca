using System.Linq;
using Code.UI.Application;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Utils;
using Code.UI.VipPass;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.InAppPurchase;
using Metaplay.Unity.DefaultIntegration;
using Metaplay.Unity.IAP;
using System.Threading;
using Game.Logic.LiveOpsEvents;
using Metaplay.Core.Offers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Shop {
	public class ShopItem : ButtonHelper {
		[SerializeField] private TMP_Text CostLabel;
		[SerializeField] private Image ItemIcon;
		[SerializeField] private TMP_Text AmountLabel;
		[SerializeField] private TMP_Text TitleLabel;

		[SerializeField] private GameObject PurchasingOverlay;
		[SerializeField] private GameObject SoldOutOverlay;

		[Inject] private AddressableManager addressableManager;
		[Inject] private IUIRootController uiRootController;

		public InAppProductInfo Product { get; private set; }

		private MetaOfferInfoBase offerInfo;
		private MetaOfferGroupModelBase offerGroup;
		private bool purchaseIsPending;
		private bool willSellOut;
		

		public async UniTask Setup(InAppProductInfo product, bool isOffer, MetaOfferInfoBase offerInfo,
			MetaOfferGroupModelBase activeGroup) {
			PurchasingOverlay.SetActive(false);
			SoldOutOverlay.SetActive(false);
			Product = product;
			this.offerInfo = offerInfo;
			offerGroup = activeGroup;
			var productMaybe = MetaplayClient.IAPManager.TryGetStoreProductInfo(Product.ConfigKey);
			if (productMaybe.HasValue)
			{
				TitleLabel.text = product.Name;
				
				if (offerInfo != null)
					TitleLabel.text = offerInfo.DisplayName;
				
				TitleLabel.gameObject.SetActive(!string.IsNullOrWhiteSpace(product.Name));
				
				IAPManager.StoreProductInfo storeProduct = productMaybe.Value;
				// Bypass fake store so we have configured price in UI
				CostLabel.text = product.Price.Float.ToString("C");
				if (product.Resources.Count > 0 || offerInfo?.Rewards.OfType<RewardCurrency>().Any() == true) {
					var multiplier = MetaplayClient.PlayerModel.LiveOpsEvents.EventModels.Values.Where(x=>x.Phase.IsActivePhase()).Select(x=>x.Content).OfType<CurrencyMultiplierEvent>().FirstOrDefault(x=>x.Type == CurrencyTypeId.Gems)?.Multiplier ?? 1;
					ItemIcon.sprite = await addressableManager.GetLazy<Sprite>(Product.Icon);
					AmountLabel.text = FindAmount(CurrencyTypeId.Gems) + (multiplier > 1 ? $" x{multiplier}!" : "");
					AmountLabel.enableWordWrapping = false;
					GetComponent<Image>().color = isOffer ? new Color(255, 200, 0) : Color.white;
				}
				else if (product.Items.Count > 0)
				{
					ItemIcon.sprite = await addressableManager.GetLazy<Sprite>(Product.Icon);
					var itemCountInfo = product.Items.First();
					AmountLabel.text = $"{itemCountInfo.Count}x " + new LevelId<ChainTypeId>(itemCountInfo.Type, itemCountInfo.Level).Localize();
					AmountLabel.enableAutoSizing = true;
					GetComponent<Image>().color = isOffer ? new Color(255, 200, 0) : Color.white;
				}
				else if (offerInfo?.Rewards.OfType<PlayerRewardItem>().Any() == true)
				{
					ItemIcon.sprite = await addressableManager.GetLazy<Sprite>(Product.Icon);
					var itemCountInfo = offerInfo.Rewards.OfType<PlayerRewardItem>().First();
					AmountLabel.text = $"{itemCountInfo.Amount}x " + itemCountInfo.ChainId.Localize();
					AmountLabel.enableAutoSizing = true;
					GetComponent<Image>().color = isOffer ? new Color(255, 200, 0) : Color.white;
				}
				else if (product.VipPassId != VipPassId.None) {
					ItemIcon.sprite = await addressableManager.GetLazy<Sprite>($"Icons/VipBadge.png");
					int daysLeft = (int) (product.VipPassDuration.Milliseconds / (24 * 3600_000));
					AmountLabel.text = Localizer.Localize("VipPass.Label.DaysLeft", daysLeft);
				}
			}
		}

		private void Update() {
			bool purchaseRunning = MetaplayClient.IAPFlowTracker.PurchaseFlowIsOngoing(Product.ConfigKey);
			PurchasingOverlay.SetActive(purchaseRunning);
			Button.interactable = !purchaseRunning || purchaseIsPending || willSellOut;
			
			if (purchaseIsPending)
			{
				InAppProductId productId = offerInfo.InAppProduct.Ref.ProductId;

				if (MetaplayClient.PlayerModel.PendingDynamicPurchaseContents.TryGetValue(productId, out PendingDynamicPurchaseContent pendingContent)
				    && pendingContent.Status == PendingDynamicPurchaseContentStatus.ConfirmedByServer)
				{
					// Initiate the actual IAP purchase, using whatever
					// IAP management mechanism is normally used by the game.
					// This should be the same mechanism as is used
					// for any other Metaplay-validated purchases.
					MetaplayClient.IAPManager.TryBeginPurchaseProduct(Product.ConfigKey);
					// Stop tracking. Normal IAP validation flow will take it from here.
					purchaseIsPending = false;
					
					SoldOutOverlay.SetActive(willSellOut);
				}
			}
		}

		protected override void OnClick() {
			OnClickShopItem();
		}

		private int FindAmount(CurrencyTypeId type) {
			if (Product.Resources != null && Product.Resources.Count > 0)
			{
				foreach (ResourceInfo resource in Product.Resources)
				{
					if (resource.Type == type)
					{
						return resource.Amount;
					}
				}
			}

			if (offerInfo != null)
			{
				return offerInfo.Rewards.OfType<RewardCurrency>()
					.FirstOrDefault(x => x.CurrencyId == CurrencyTypeId.Gems)?.Amount ?? 0; 
			}

			return 0;
		}

		private void OnClickShopItem() {
			if (Product.Resources.Count > 0 || Product.Items.Count > 0 || offerInfo?.Rewards?.OfType<RewardCurrency>().Any() == true || offerInfo?.Rewards?.OfType<PlayerRewardItem>().Any() == true) {
				if (offerGroup != null)
				{
					if (offerInfo.PerPlayerPurchaseLimitReached(MetaplayClient.PlayerModel.MetaOfferGroups.GetOfferNumPurchasedByPlayer(offerInfo.OfferId) + 1) || 
					    offerInfo.PerGroupPurchaseLimitReached(offerGroup.GetOfferNumPurchasedInGroup(offerInfo.OfferId) + 1))
					{
						willSellOut = true;
					}
					
					MetaplayClient.PlayerContext.ExecuteAction(new PlayerPreparePurchaseMetaOffer(
						offerGroup.ActivableInfo,
						offerInfo, 
						null));
					purchaseIsPending = true;
				}
				else
					MetaplayClient.IAPManager.TryBeginPurchaseProduct(Product.ConfigKey);
			} else {
				uiRootController.ShowUI<VipPassPopup, VipPassPopupPayload>(
					new VipPassPopupPayload(Product.ConfigKey),
					CancellationToken.None
				);
			}
		}
	}
}
