using System;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.InAppPurchase;
using Metaplay.Unity.DefaultIntegration;
using Metaplay.Unity.IAP;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.VipPass {
	public class VipPassPopup : UIRootBase<VipPassPopupPayload> {
		[SerializeField] private TMP_Text PurchaseLabel;
		[SerializeField] private GameObject PurchasingOverlay;
		[SerializeField] private RectTransform PerkContainer;
		[SerializeField] private VipPassPerk PerkTemplate;
		[SerializeField] private Button PurchaseButton;
		[SerializeField] private Button CloseButton;

		[Inject] private AddressableManager addressableManager;
		[Inject] private DiContainer diContainer;

		private bool purchaseStarted;

		protected override void Init() {
			IAPManager.StoreProductInfo? productMaybe = MetaplayClient.IAPManager.TryGetStoreProductInfo(UIHandle.Product);
			if (productMaybe.HasValue) {
				IAPManager.StoreProductInfo product = productMaybe.Value;
				InAppProductInfo productInfo = MetaplayClient.PlayerModel.GameConfig.InAppProducts[UIHandle.Product];
				int days = (int) (productInfo.VipPassDuration.Milliseconds / (24 * 3600_000));
				PurchaseLabel.text = Localizer.Localize(
					"VipPass.PurchaseLabel",
					days,
					product.Product.metadata.localizedPriceString
				);
				VipPassInfo vipPassInfo = MetaplayClient.PlayerModel.GameConfig.VipPasses[productInfo.VipPassId];
				SetupPerk("Icon_EnergyPack", Localizer.Localize("VipPass.PerkMaxEnergy", vipPassInfo.MaxEnergyBoost))
					.Forget();
				SetupPerk(
					"Gems",
					Localizer.Localize(
						"VipPass.PerkGems",
						vipPassInfo.DailyRewardResources.Find(r => r.Type == CurrencyTypeId.Gems).Amount
					)
				).Forget();
				SetupPerk(
					"Icon_Builder",
					Localizer.Localize(
						"VipPass.PerkBuildTime",
						Math.Round(100 * (1 - vipPassInfo.BuilderTimerFactor.Float))
					)
				).Forget();
				SetupPerk("Icon_Map", Localizer.Localize("VipPass.PerkEnergyIsland")).Forget();
				SetupPerk("Icon_Builder", Localizer.Localize("VipPass.PerkExtraBuilder")).Forget();
			}
		}

		protected override async UniTask Idle(CancellationToken ct) {
			(_, bool purchaseRequested) = await UniTask.WhenAny(
				new[] {
					PurchaseButton.OnClickAsync(ct).ContinueWith(() => true),
					OnDismissAsync(ct).ContinueWith(() => false)
				}
			);

			if (purchaseRequested) {
				MetaplayClient.IAPManager.TryBeginPurchaseProduct(UIHandle.Product);
				PurchasingOverlay.SetActive(true);
				await UniTask.WaitWhile(
					() => MetaplayClient.IAPFlowTracker.PurchaseFlowIsOngoing(UIHandle.Product),
					PlayerLoopTiming.Update,
					ct
				);
				PurchasingOverlay.SetActive(false);
			}
		}

		private UniTask OnDismissAsync(CancellationToken ct) {
			return UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}

		private async UniTask SetupPerk(string icon, string label) {
			Sprite iconSprite = await addressableManager.GetLazy<Sprite>($"Icons/{icon}.png");
			VipPassPerk perk = diContainer.InstantiatePrefabForComponent<VipPassPerk>(PerkTemplate, PerkContainer);
			perk.Setup(iconSprite, label);
		}
	}

	public class VipPassPopupPayload : UIHandleBase {
		public InAppProductId Product { get; }

		public VipPassPopupPayload(InAppProductId product) {
			Product = product;
		}
	}
}
