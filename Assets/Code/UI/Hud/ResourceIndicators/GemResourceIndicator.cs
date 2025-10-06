using System.Threading;
using Code.UI.Core;
using Code.UI.HudBase;
using Code.UI.Shop;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using Zenject;

namespace Code.UI.Hud.ResourceIndicators {
	public class GemResourceIndicator : ResourceIndicatorBase {
		protected override int ResourceAmount => MetaplayClient.PlayerModel.Wallet.Gems.Value;
		protected override CurrencyTypeId Type => CurrencyTypeId.Gems;

		[Inject] private IUIRootController uiRootController;

		public override void OnClick() {
			// We use shop opening in the demo flow
			// if (MetaplayClient.PlayerModel.PrivateProfile.FeaturesEnabled.Contains(FeatureTypeId.HudButtonGems)) {
			// 	uiRootController.ShowUI<ShopUIRoot, ShopUIHandle>(
			// 		new ShopUIHandle(new ShopUIHandle.ShopNavigationPayload()),
			// 		CancellationToken.None
			// 	);
			// }
		}
	}
}
