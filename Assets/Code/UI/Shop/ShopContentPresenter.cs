using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Offers;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Shop {
	public class ShopContentPresenter : MonoBehaviour {
		[SerializeField] private RectTransform ShopContent;
		[SerializeField] private ShopItem ShopItemTemplate;
		[SerializeField] private GameObject Overlay;
		[SerializeField] private TMP_Text OverlayLabel;

		[Inject] private DiContainer diContainer;

		protected void Awake() {
			Clear();
			Overlay.SetActive(true);
			OverlayLabel.text = "Initializing shop...";
		}

		public void Clear() {
			foreach (Transform child in ShopContent) {
				Destroy(child.gameObject);
			}

			iapShopInitialized = false;
		}

		private bool iapShopInitialized = false;

		private void LateUpdate() {
			if (iapShopInitialized) {
				return;
			}

			// Player entered the shop; refresh offers.
			// To avoid unnecessary workload on the server, only execute the refresh
			// action if there's actually something to do, and only instruct it to
			// refresh the offers that can be refreshed.
			MetaOfferGroupsRefreshInfo refreshInfo = MetaplayClient.PlayerModel.GetMetaOfferGroupsRefreshInfo();
			if (refreshInfo.HasAny())
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerRefreshMetaOffers(refreshInfo));

			PlayerModel player = MetaplayClient.PlayerModel;

			// Here, we only show offer groups with the placement Shop.
			OfferPlacementId shopPlacement = OfferPlacementId.FromString("Shop");

			// Find all products that are part of an offer group in the Shop placement.
			List<InAppProductInfo> offerProducts = new List<InAppProductInfo>();

			foreach (DefaultMetaOfferGroupInfo offerGroup in player.GameConfig.OfferGroups.Values
						.Where(g => g.Placement == shopPlacement))
			{
				foreach (MetaOfferInfoBase offer in offerGroup.Offers.MetaRefUnwrap()) {
					InAppProductInfoBase maybeInAppProduct = offer.InAppProduct.MaybeRef;
					if (maybeInAppProduct != null && maybeInAppProduct is InAppProductInfo inAppProduct) {
						offerProducts.Add(inAppProduct);
					}
				}
			}

			// Find the active offer group in the Shop placement for player, if any.
			IEnumerable<MetaOfferGroupModelBase> activeGroups = player.MetaOfferGroups
				.GetActiveStates(player)
				.Where(group => group.ActivableInfo.Placement == shopPlacement);

			if (MetaplayClient.IAPManager.StoreIsAvailable) {
				iapShopInitialized = true;
				IEnumerable<InAppProductInfo> products = MetaplayClient.PlayerModel
					.GameConfig.InAppProducts.Values
					.Where(productInfo => MetaplayClient.IAPManager.StoreProductIsAvailable(productInfo.ProductId));

				foreach (var activeGroup in activeGroups)
				{
					foreach (MetaOfferInfoBase offerInfo in activeGroup.ActivableInfo.Offers.MetaRefUnwrap())
					{
						InAppProductInfo productInfo = (InAppProductInfo)offerInfo.InAppProduct.MaybeRef;
						if (productInfo != null &&
							(productInfo.Resources.Count > 0 || 
							 productInfo.Items.Count > 0 ||
							 productInfo.VipPassId != VipPassId.None || productInfo.HasDynamicContent)) {
							SpawnShopItem(ShopContent, productInfo, offerInfo, activeGroup, true);
						}
					}
				}

				// Show all products that are not part of an offer group.
				foreach (InAppProductInfo productInfo in products.Where(p => !offerProducts.Contains(p))) {
					if (productInfo.Resources.Count > 0 ||
						productInfo.VipPassId != VipPassId.None) {
						SpawnShopItem(ShopContent, productInfo, null, null, false);
					}
				}

				foreach (var layoutGroup in ShopContent.GetComponentsInChildren<LayoutGroup>()) {
					LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
				}

				Overlay.SetActive(false);
			} else if (MetaplayClient.IAPManager.StoreInitFailure.HasValue) {
				iapShopInitialized = true;
				OverlayLabel.text =
					$"Shop initialization failed: {MetaplayClient.IAPManager.StoreInitFailure.Value.Reason}";
			}
		}

		private void SpawnShopItem(Transform container, InAppProductInfo product, MetaOfferInfoBase offerInfo, MetaOfferGroupModelBase activeGroup, bool isOffer) {
			ShopItem shopItem = diContainer.InstantiatePrefab(ShopItemTemplate, container).GetComponent<ShopItem>();
			shopItem.Setup(product, isOffer, offerInfo, activeGroup).Forget();
		}
	}
}
