using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.InAppPurchase;
using Metaplay.Unity.DefaultIntegration;
using Metaplay.Unity.IAP;
using System.Threading;
using Code.UI.Offers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI {
	public class OfferPopup : UIRootWithResultBase<OfferPopupHandle, OfferPopupResult> {
		[SerializeField] private TMP_Text Title;
		[SerializeField] private Image Icon;
		[SerializeField] private TMP_Text PurchaseLabel;
		[SerializeField] private GameObject PurchasingOverlay;
		[SerializeField] private Button PurchaseButton;
		[SerializeField] private Button CloseButton;
		[SerializeField] private Button CancelButton;

		[SerializeField] private OfferItem OfferItemTemplate;
		[SerializeField] private RectTransform OfferItemContainer;

		[Inject] private AddressableManager addressableManager;
		[Inject] private IUIRootController uiRootController;
		[Inject] private DiContainer diContainer;

		private bool purchaseStarted;

		protected override void Init() {
			Setup().Forget();
		}

		private async UniTask Setup() {
			IAPManager.StoreProductInfo? productMaybe =
				MetaplayClient.IAPManager.TryGetStoreProductInfo(UIHandle.Product);
			if (productMaybe.HasValue) {
				IAPManager.StoreProductInfo product = productMaybe.Value;
#if false // [petri] disabled as it doesn't compile
				Title.text =
					MetaplayClient.PlayerModel.GameConfig.InAppProducts.GetInfoByKey(UIHandle.Product) is
						InAppProductInfo inAppProductInfo
						? inAppProductInfo.Name
						: product.Product.metadata.localizedTitle;
#else
				Title.text = MetaplayClient.PlayerModel.GameConfig.InAppProducts.GetInfoByKey(UIHandle.Product) is InAppProductInfo inAppProductInfo ?
					inAppProductInfo.Name : null; //product.Product.metadata.localizedTitle;
#endif
				PurchaseLabel.text = product.Product.metadata.localizedPriceString;
				InAppProductInfo productInfo = MetaplayClient.PlayerModel.GameConfig.InAppProducts[UIHandle.Product];
				Icon.sprite = await addressableManager.GetLazy<Sprite>(productInfo.Icon);

				foreach (ResourceInfo resource in productInfo.Resources) {
					OfferItem offerItem = diContainer.InstantiatePrefabForComponent<OfferItem>(
						OfferItemTemplate,
						OfferItemContainer
					);
					offerItem.Setup(resource.Type, resource.Amount);
				}

				foreach (ItemCountInfo item in productInfo.Items) {
					OfferItem offerItem = diContainer.InstantiatePrefabForComponent<OfferItem>(
						OfferItemTemplate,
						OfferItemContainer
					);
					offerItem.Setup(item.Type, item.Level, item.Count);
				}
			}
		}

		protected override async UniTask<OfferPopupResult> IdleWithResult(CancellationToken ct) {
			(_, bool success) = await UniTask.WhenAny(
				new[] {
					WaitForConfirmAsync(ct).ContinueWith(() => true),
					WaitForDeclineAsync(ct).ContinueWith(() => false)
				}
			);

			if (success) {
				MetaplayClient.IAPManager.TryBeginPurchaseProduct(UIHandle.Product);
				PurchasingOverlay.SetActive(true);
				await UniTask.WaitWhile(
					() => MetaplayClient.IAPFlowTracker.PurchaseFlowIsOngoing(UIHandle.Product),
					cancellationToken: ct
				);
				PurchasingOverlay.SetActive(false);
				return new OfferPopupResult(OfferPopupResponse.Yes);
			}

			if (!UIHandle.LastTimeOffer) {
				return new OfferPopupResult(OfferPopupResponse.No);
			}

			ConfirmationPopupHandle payload = uiRootController.ShowUI<ConfirmationPopup, ConfirmationPopupHandle>(
				new ConfirmationPopupHandle(
					Localizer.Localize("Offer.LastTimeOffer.Title"),
					Localizer.Localize("Offer.LastTimeOffer.Content"),
					ConfirmationPopupHandle.ConfirmationPopupType.YesNo
				),
				CancellationToken.None
			);
			ConfirmationPopupResult result = await payload.OnResult;

			if (result.Response == ConfirmationPopupResponse.Yes) {
				return new OfferPopupResult(OfferPopupResponse.No);
			}

			// We re-run the flow as the cancelled the cancellation
			return await IdleWithResult(ct);
		}


		private UniTask WaitForConfirmAsync(CancellationToken ct) {
			return PurchaseButton.OnClickAsync(ct);
		}

		private UniTask WaitForDeclineAsync(CancellationToken ct) {
			return UniTask.WhenAny(
				CancelButton.OnClickAsync(ct),
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}
	}

	public class OfferPopupHandle : UIHandleWithResultBase<OfferPopupResult> {
		public InAppProductId Product { get; }
		public bool LastTimeOffer { get; }

		public OfferPopupHandle(InAppProductId product, bool lastTimeOffer) {
			Product = product;
			LastTimeOffer = lastTimeOffer;
		}
	}

	public class OfferPopupResult : IUIResult {
		public OfferPopupResult(OfferPopupResponse response) {
			Response = response;
		}

		public OfferPopupResponse Response { get; }
	}

	public enum OfferPopupResponse {
		Yes,
		No
	}
}
