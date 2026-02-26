using Code.UI;
using Code.UI.Events;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System.Threading;
using Code.UI.Application.Signals;
using Code.UI.Deletion;
using Zenject;

namespace Code.Game {
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class GameFlowController : IInitializable {
		private readonly UIController uiController;
		private readonly IEventsFlowController eventsFlowController;
		private readonly IDeletionController deletionController;
		private readonly CancellationToken ct;
		[Inject] private SignalBus signalBus;

		public GameFlowController(
			UIController uiController,
			IEventsFlowController eventsFlowController,
			IDeletionController deletionController,
			CancellationToken ct
		) {
			this.uiController = uiController;
			this.eventsFlowController = eventsFlowController;
			this.deletionController = deletionController;
			this.ct = ct;
		}

		public void Initialize() {
			InitializeAsync().Forget();

			signalBus.Subscribe<ApplicationPauseSignal>(OnApplicationPause);
			signalBus.Subscribe<ApplicationFocusSignal>(OnApplicationFocus);
		}

		private void OnApplicationFocus(ApplicationFocusSignal signal) {
			#if !UNITY_EDITOR && UNITY_WEBGL
			GameWebGLApiBridge.UnityApplicationFocused(signal.Focused);
			#endif
		}

		private void OnApplicationPause(ApplicationPauseSignal signal) {
			#if !UNITY_EDITOR && UNITY_WEBGL
			GameWebGLApiBridge.UnityApplicationPaused(signal.Paused);
			#endif
		}

		private async UniTask InitializeAsync() {
			uiController.Startup();
			if (deletionController.IsScheduled) {
				await deletionController.ShowPopup(ct);
			}
			eventsFlowController.ShowStartupAdvertisements(ct).Forget();
		}
	}
}
