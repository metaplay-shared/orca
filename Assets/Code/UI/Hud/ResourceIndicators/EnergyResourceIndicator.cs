using System.Threading;
using Code.UI.Core;
using Code.UI.HudBase;
using Code.UI.Market;
using Code.UI.Shop;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.Hud.ResourceIndicators {
	public class EnergyResourceIndicator : ResourceIndicatorBase {
		[SerializeField] private GameObject TimerRoot;
		[SerializeField] private TMP_Text TimerText;

		[Inject] private IUIRootController uiRootController;

		protected override int ResourceAmount => MetaplayClient.PlayerModel.Merge.Energy.ProducedAtUpdate;
		protected override CurrencyTypeId Type => CurrencyTypeId.Energy;

		public override void OnClick() {
			// We use shop opening in the demo flow
			// if (MetaplayClient.PlayerModel.PrivateProfile.FeaturesEnabled.Contains(FeatureTypeId.HudButtonEnergy)) {
			// 	uiRootController.ShowUI<ShopUIRoot, ShopUIHandle>(
			// 		new ShopUIHandle(new ShopUIHandle.MarketNavigationPayload(ShopCategoryId.Energy)),
			// 		CancellationToken.None
			// 	);
			// }
		}

		private void Update() {
			PlayerModel player = MetaplayClient.PlayerModel;
			if (player.Merge.Energy.ProducedAtUpdate >= player.MaxEnergy) {
				TimerRoot.gameObject.SetActive(false);
				return;
			}

			TimerRoot.gameObject.SetActive(true);
			TimerText.text = player.Merge
				.TimeToNext(player.CurrentTime, player.EnergyGeneratedPerHour, player.MaxEnergy).ToSimplifiedString();
		}
	}
}
