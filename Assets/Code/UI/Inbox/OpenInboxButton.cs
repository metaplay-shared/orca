using Code.UI.Core;
using Code.UI.Utils;
using Metaplay.Unity.DefaultIntegration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Code.UI.Inbox {
	public class OpenInboxButton : ButtonHelper {
		[SerializeField] private GameObject NewIndicator;

		private IUIRootController uiRootController;

		private void Start() {
			NewIndicator.SetActive(MetaplayClient.PlayerModel.MailInbox.Count(m => m.IsRead == false) > 0);
		}

		[Inject]
		[SuppressMessage("ReSharper", "ParameterHidesMember")]
		private void Inject(
			IUIRootController uiRootController
		) {
			this.uiRootController = uiRootController;
		}

		protected override void OnClick() {
			uiRootController.ShowUI<InboxPopup, InboxPopupHandle>(new InboxPopupHandle(), CancellationToken.None);
			NewIndicator.SetActive(false);
		}
	}
}
