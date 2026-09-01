using JetBrains.Annotations;
using Metaplay.Core.InGameMail;
using Metaplay.Core.Player;
using Metaplay.Unity.DefaultIntegration;
using System.Linq;
using UniRx;

namespace Code.Inbox {
	public interface IInboxController {
		IReadOnlyReactiveProperty<bool> HasUnreadMessages { get; }
		void NotifyNewInGameMail(PlayerMailItem mail);
		void MarkInGameMailRead();
		void NotifyMailStateChanged(PlayerMailItem mail);
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class InboxController : IInboxController {
		public InboxController() {
			HasUnreadMessages = new ReactiveProperty<bool>(GetHasUnreadMessages());
		}

		private IReactiveProperty<bool> HasUnreadMessages { get; }

		public void NotifyNewInGameMail(PlayerMailItem mail) {
			UpdateState();
		}

		private void UpdateState() {
			HasUnreadMessages.Value = GetHasUnreadMessages();
		}

		public void MarkInGameMailRead() {
			foreach (var mail in MetaplayClient.PlayerModel.MailInbox) {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerToggleMailIsRead(mail.Id, true));
			}
		}

		public void NotifyMailStateChanged(PlayerMailItem mail) {
			UpdateState();
		}

		private bool GetHasUnreadMessages() {
			return MetaplayClient.PlayerModel.MailInbox.Any(
				mail => !mail.IsRead || (mail.Contents.MustBeConsumed && !mail.HasBeenConsumed)
			);
		}

		IReadOnlyReactiveProperty<bool> IInboxController.HasUnreadMessages => HasUnreadMessages;
	}
}
