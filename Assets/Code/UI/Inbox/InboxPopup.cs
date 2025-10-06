using Code.Inbox;
using System.Collections.Generic;
using System.Threading;
using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Metaplay.Core.InGameMail;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Inbox {
	public class InboxPopupHandle : UIHandleBase { }

	public class InboxPopup : UIRootBase<InboxPopupHandle> {
		[SerializeField] private RectTransform InboxItemsContainer;
		[SerializeField] private InboxItem PrefabInboxItem;
		[SerializeField] private Button CloseButton;

		[Inject] private DiContainer container;
		[Inject] private IInboxController inboxController;

		private void SetupContent(List<PlayerMailItem> inboxItems) {
			foreach (PlayerMailItem item in inboxItems) {
				InboxItem inboxItem =
					container.InstantiatePrefabForComponent<InboxItem>(PrefabInboxItem, InboxItemsContainer);
				inboxItem.Setup(item);
			}
		}

		private void Clear() {
			foreach (Transform child in InboxItemsContainer) {
				Destroy(child.gameObject);
			}
		}

		protected override void Init() {
			inboxController.MarkInGameMailRead();
			Clear();
			SetupContent(MetaplayClient.PlayerModel.MailInbox);
		}

		protected override UniTask Idle(CancellationToken ct) {
			return UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}
	}
}
