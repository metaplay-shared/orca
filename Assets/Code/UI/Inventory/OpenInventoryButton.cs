using System.Threading;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Code.UI.Inventory {
	public class OpenInventoryButton : ButtonHelper {
		[Inject] private IUIRootController uiRootController;

		protected override void OnClick() {
			uiRootController.ShowUI<InventoryPopup, InventoryPopupPayload>(new(), CancellationToken.None);
		}
	}
}
