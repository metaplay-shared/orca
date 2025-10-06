using System;
using System.Threading;
using Code.UI.Application;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Metaplay.Core;
using Metaplay.Core.Player;
using Metaplay.Unity;
using Metaplay.Unity.DefaultIntegration;
using Zenject;

namespace Code.UI.Deletion {
	public interface IDeletionController {
		void CancelDeletion();
		void ScheduledForDeletionChanged();
		MetaDuration TimeLeft { get; }
		bool IsScheduled { get; }
		UniTask WaitForCancellation(CancellationToken ct);
		UniTask ShowPopup(CancellationToken ct);
		UniTask TryConfirmDeletion(CancellationToken ct);
	}

	public class DeletionController : IDeletionController {
		[Inject] private IApplicationStateManager applicationStateManager;
		[Inject] private IUIRootController uiRootController;
		private UniTaskCompletionSource tcs;

		public void CancelDeletion() {
			MetaplaySDK.MessageDispatcher.SendMessage(new PlayerCancelScheduledDeletionRequest());
		}

		public void ScheduledForDeletionChanged() {
			if (!IsScheduled && tcs != null) {
				tcs.TrySetResult();
			} else {
				applicationStateManager.SwitchToState(ApplicationState.Initializing, CancellationToken.None).Forget();
			}
		}

		public MetaDuration TimeLeft => MetaplayClient.PlayerModel.ScheduledForDeletionAt - MetaTime.Now;
		public bool IsScheduled => MetaplayClient.PlayerModel.DeletionStatus != PlayerDeletionStatus.None;

		public async UniTask WaitForCancellation(CancellationToken ct) {
			tcs = new UniTaskCompletionSource();
			using IDisposable disposable = ct.Register(() => tcs.TrySetCanceled());
			await tcs.Task;
		}

		public UniTask ShowPopup(CancellationToken ct) {
			var handle = uiRootController.ShowUI<DeletionScheduledUIRoot, DeletionScheduledPayload>(
				new DeletionScheduledPayload(),
				ct
			);
			return handle.OnComplete;
		}

		public async UniTask TryConfirmDeletion(CancellationToken ct) {
			ConfirmationPopupHandle handle = uiRootController.ShowUI<ConfirmationPopup, ConfirmationPopupHandle>(
				new ConfirmationPopupHandle(
					Localizer.Localize("Info.DeletionConfirmation"),
					Localizer.Localize("Info.DeletionConfirmationDetail"),
					ConfirmationPopupHandle.ConfirmationPopupType.YesNo
				),
				ct
			);

			ConfirmationPopupResult result = await handle.OnCompleteWithResult;

			if (result.Response == ConfirmationPopupResponse.Yes) {
				// MetaplaySDK.MessageDispatcher.SendMessage(new PlayerScheduleDeletionRequest());
				await new CredentialsStore("").ClearGuestCredentialsAsync();
			}
		}
	}
}
