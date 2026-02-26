using System.Threading;
using Code.UI.Core;
using Code.UI.Market;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Zenject;

namespace Code.UI.Shop {
	public class OpenShopButton : ButtonHelper {
		[Inject] private IUIRootController uiRootController;

		protected override void OnClick() {
			uiRootController.ShowUI<ShopUIRoot, ShopUIHandle>(
				new ShopUIHandle(
					new ShopUIHandle.ShopNavigationPayload()
				),
				CancellationToken.None
			);
		}
	}
}
