using Code.UI.Utils;
using Game.Logic;
using Metaplay.Core.InAppPurchase;
using Metaplay.Unity.DefaultIntegration;
using Metaplay.Unity.IAP;
using UnityEngine;

namespace Code.UI.Merge.AddOns.MergeBoard.LockArea {
	public class LockAreaState : MonoBehaviour {
		[SerializeField] private GameObject LockIcon;
		[SerializeField] private LockAreaDetails Details;
		[SerializeField] private CurrencyLabel Cost;
		[SerializeField] private GameObject VipPassIcon;
		[SerializeField] private GameObject UnlockDetails;
		[SerializeField] public GameObject TapText;

		private LockAreaInfo areaInfo;

		public void Setup(LockAreaInfo info) {
			areaInfo = info;
			AreaState state = MetaplayClient.PlayerModel.Islands[info.IslandId].MergeBoard.LockArea.Areas.GetValueOrDefault(areaInfo.AreaIndex);
			ChangeState(state);
			Details.Setup(info.PlayerLevel, info.Hero, info.HeroLevel);
			Details.gameObject.SetActive(false);
			VipPassIcon.SetActive(false);
			if (info.UnlockProduct == InAppProductId.FromString("None")) {
				Cost.Set(
					info.UnlockCost.Type,
					info.UnlockCost.Amount,
					info.UnlockCost.Type == CurrencyTypeId.TrophyTokens
				);
			} else {
				IAPManager.StoreProductInfo? productMaybe = MetaplayClient.IAPManager.TryGetStoreProductInfo(info.UnlockProduct);
				if (productMaybe.HasValue) {
					var product = productMaybe.Value;
					InAppProductInfo productInfo =
						MetaplayClient.PlayerModel.GameConfig.InAppProducts[info.UnlockProduct];
					if (productInfo.VipPassId == VipPassId.None) {
						Cost.Set(product.Product.metadata.localizedPriceString);
					} else {
						Cost.gameObject.SetActive(false);
						VipPassIcon.SetActive(true);
					}
				} else {
					Cost.Set(Localizer.Localize("Info.ComingSoon"));
				}
			}
		}

		public void ChangeState(AreaState state) {
			if (state == AreaState.Locked) {
				PlayerModel player = MetaplayClient.PlayerModel;
				SharedGameConfig gameConfig = player.GameConfig;
				LockAreaModel lockAreaModel = player.Islands[areaInfo.IslandId].MergeBoard.LockArea;
				bool dependenciesOpen = lockAreaModel.DependenciesOpen(
					areaInfo.AreaIndex,
					areaInfo.IslandId,
					gameConfig
				);

				LockIcon.SetActive(dependenciesOpen && MetaplayClient.PlayerModel.GameConfig.Client.ShowLockAreaLocks);
				Details.gameObject.SetActive(dependenciesOpen && !MetaplayClient.PlayerModel.GameConfig.Client.ShowLockAreaLocks);
				UnlockDetails.SetActive(false);
			} else if (state == AreaState.Opening) {
				LockIcon.SetActive(false);
				Details.gameObject.SetActive(false);
				UnlockDetails.SetActive(true);
			}
		}

		public void HandleClick() {
			if (LockIcon.gameObject.activeSelf) {
				LockIcon.SetActive(false);
				Details.gameObject.SetActive(true);
			}
		}
	}
}
