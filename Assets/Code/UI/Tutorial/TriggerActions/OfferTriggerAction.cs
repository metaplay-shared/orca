using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Metaplay.Core.InAppPurchase;
using System.Threading;
using Zenject;

namespace Code.UI.Tutorial.TriggerActions {
	public class OfferTriggerAction : TriggerAction {
		[Inject] private IUIRootController uiRootController;

		private readonly InAppProductId product;

		public OfferTriggerAction(InAppProductId product) {
			this.product = product;
		}
		public override UniTask Run() {
			return uiRootController.ShowUI<OfferPopup, OfferPopupHandle>(
				new OfferPopupHandle(product, true),
				CancellationToken.None
			).OnComplete;
		}
	}
}
