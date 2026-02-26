using Code.ATT;
using Code.Privacy;
using System.Threading;
using Code.UI.AssetManagement;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Unity;
using Metaplay.Unity.DefaultIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metaplay.Core;
using UnityEngine;
using UnityEngine.U2D;
using Unity.Services.Core;
using Zenject;
using Object = UnityEngine.Object;

namespace Code.UI.Application {
	public interface IApplicationStateManager {
		UniTask SwitchToState(ApplicationState newState, CancellationToken ct);
	}

	/// <summary>
	/// Manages the application's lifecycle, including mock loading state, Metaplay server connectivity, and failure states.
	/// This class is a simplified version of a state manager that a real game would have, but in such a manner that the
	/// integration of Metaplay into such a state manager is exemplified.
	///
	/// Also implements <see cref="IMetaplayLifecycleDelegate"/> to get callbacks from Metaplay on connectivity events and
	/// error states.
	/// </summary>
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class ApplicationStateManager : IApplicationStateManager {
		private readonly AddressableManager addressableManager;
		private readonly IPrivacyController privacyController;
		private readonly IATTController attController;
		private readonly LogChannel logChannel;

		public ApplicationStateManager(
			AddressableManager addressableManager,
			IPrivacyController privacyController,
			IATTController attController
		) {
			this.addressableManager = addressableManager;
			this.privacyController = privacyController;
			this.attController = attController;
			logChannel = MetaplaySDK.Logs.CreateChannel("Application state manager");
		}

		// Runtime state
		private ApplicationState applicationState = ApplicationState.AppStart; // Begin in the AppStart state.

		private readonly ApplicationSceneManager applicationSceneManager = new();

		/// <summary>
		/// Switch the application's state and perform actions relevant to the state transition.
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="ct"></param>
		public async UniTask SwitchToState(ApplicationState newState, CancellationToken ct) {
			logChannel.Info($"Switching to state {newState} (from {applicationState})");

			switch (newState) {
				case ApplicationState.AppStart:
					// Cannot enter, app starts in this state.
					break;

				case ApplicationState.Initializing:
					await UniTask.WaitForEndOfFrame();

					try {
						await UnityServices.InitializeAsync();
					}
					catch (Exception e) {
						logChannel.Error($"Unity Services initialization failed: {e}");
					}

					await applicationSceneManager.SwitchGameState(newState);

					// await privacyController.AcceptTermsOfServiceAsync(ct);
					// attController.RequestAuthorizationTracking();

					Dictionary<object, float> progresses = new();

					IProgress<float> GetProgressInstance() {
						Progress<float> progress = new();
						progresses.Add(progress, 0f);
						progress.ProgressChanged += (sender, value) => progresses[sender] = value;
						return progress;
					}

					LoadingInfo.ProgressGetter = () => {
						float progress = progresses.Values.Sum() / progresses.Values.Count;
						return progress;
					};

					logChannel.Info("Loading assets...");

					addressableManager.PreloadSetAsync<Object>("LazyPersistent").Forget();
					await UniTask.WhenAll(
						addressableManager.PreloadSetAsync<SpriteAtlas>("Chains", GetProgressInstance()),
						addressableManager.PreloadSetAsync<Sprite>("Heroes", GetProgressInstance()),
						addressableManager.PreloadSetAsync<Sprite>("Currencies", GetProgressInstance()),
						addressableManager.PreloadSetAsync<Sprite>("MergeBorder", GetProgressInstance()),
						addressableManager.PreloadSetAsync<Sprite>("Lockarea", GetProgressInstance())
					);

					// Start connecting to the server.
					logChannel.Info("Connecting to Metaplay server...");
					if (MetaplayClient.ConnectionHealth == ConnectionHealth.NotConnected) {
						MetaplayClient.Connect();
					} else {
						SwitchToState(ApplicationState.Game, ct).Forget();
					}

					break;

				case ApplicationState.Game:
					var userId = MetaplayClient.PlayerModel.PlayerId.ToString();

					await UniTask.WaitUntil(
						() => MetaplayClient.PlayerModel.CurrentTick > 0,
						PlayerLoopTiming.Update,
						ct
					);

					// Start the game. Simulate the transition to in-game state by spawning the GameManager.
					// You might want to use scene transition instead.
					await applicationSceneManager.SwitchGameState(newState);
					await UniTask.WaitUntil(() => ApplicationInstaller.IsReady, cancellationToken: ct);

					SceneContext sceneContext = Object.FindObjectOfType<SceneContext>();
					PlayerModelClientListener playerModelClientListener =
						sceneContext.Container.Resolve<PlayerModelClientListener>();
					MetaplayClient.PlayerModel.ClientListener = playerModelClientListener;
					MetaplayClient.PlayerModel.ClientListenerCore = playerModelClientListener;

					Debug.Log("Sending init action...");
					MetaplayClient.PlayerContext.ExecuteAction(new PlayerInitGameAction());


#if UNITY_WEBGL && !UNITY_EDITOR
					if (!string.IsNullOrEmpty(MetaplayClient.PlayerModel.LatestInfoUrl))
						GameWebGLApiBridge.UpdateInfoUrl(MetaplayClient.PlayerModel.LatestInfoUrl);
#endif
					break;
			}

			// Store the new state.
			applicationState = newState;
		}
	}
}
