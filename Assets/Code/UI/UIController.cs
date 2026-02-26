using System.Threading;
using Code.UI.Application;
using Code.UI.CloudCurtains;
using Code.UI.Core.UIBlock;
using Code.UI.Island.Signals;
using Code.UI.Map;
using Code.UI.Map.Signals;
using Code.UI.MergeBase;
using Code.UI.Tutorial;
using Code.UI.Tutorial.TriggerActions;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using System;
using Code.UI.Application.Signals;
using UnityEngine;
using Zenject;

namespace Code.UI {
	[RequireComponent(typeof(CameraControls))]
	public class UIController : MonoBehaviour {
		[SerializeField] private GameObject Background;

		[Inject] private SignalBus signalBus;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private TriggerQueue triggerQueue;
		[Inject] private CameraControls cameraControls;
		[Inject] private MergeBoardRoot mergeBoardRoot;
		[Inject] private IWorldMapBehaviour worldMapBehaviour;
		[Inject] private ICloudCurtainsPresenter cloudCurtainsPresenter;
		[Inject] private IUIBlockController uiBlockController;

		private void Awake() {
			cameraControls = GetComponent<CameraControls>();
			cameraControls.SetActive(false);

			Background.SetActive(false);
		}

		private void OnApplicationPause(bool pauseStatus) {
			signalBus.Fire(new ApplicationPauseSignal(pauseStatus));
		}

		private void OnApplicationFocus(bool hasFocus) {
			signalBus.Fire(new ApplicationFocusSignal(hasFocus));
		}

		public void Startup() {
			signalBus.Subscribe<MapControlsZoomedCloseToIslandSignal>(OnMapControlsZoomedCloseToIsland);
			signalBus.Subscribe<IslandFocusedSignal>(OnIslandFocused);
			signalBus.Subscribe<IslandHighlightedSignal>(OnIslandHighlighted);
			signalBus.Subscribe<IslandPointedSignal>(OnIslandPointed);

			var island = MetaplayClient.PlayerModel.LastIsland != IslandTypeId.None
				? MetaplayClient.PlayerModel.LastIsland
				: IslandTypeId.MainIsland;
			IslandInfo islandInfo = MetaplayClient.PlayerModel.GameConfig.Islands[island];
			cameraControls.FocusIsland(islandInfo);
			EnterIslandInstant(island);
		}

		public async UniTask LeaveIsland(CancellationToken ct) {
			using IDisposable uiBlock = uiBlockController.SetState(UIBlockState.Blocked);

			await cloudCurtainsPresenter.Close(ct);

			mergeBoardRoot.gameObject.SetActive(false);
			Background.SetActive(false);

			cameraControls.SetActive(true);
			await UniTask.WhenAll(
				cameraControls.LeaveIsland(
					MetaplayClient.PlayerModel.GameConfig.Islands[applicationInfo.ActiveIsland.Value],
					ct
				),
				cloudCurtainsPresenter.Open(ct)
			);
			IslandTypeId island = applicationInfo.ActiveIsland.Value;
			applicationInfo.ActiveIsland.Value = null;
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerEnterMap(island));

			if (worldMapBehaviour.GetNextIslandToReveal().HasValue) {
				Debug.Log("Trying to run island appearence action");
				triggerQueue.EnqueueAction(new HighlightIslandAppearingAction());
			} else {
				Debug.Log("No appearance to show");
			}

			signalBus.Fire(new EnteredMapSignal());
		}

		public async UniTask EnterIslandAsync(IslandTypeId islandId, CancellationToken ct) {
			using IDisposable uiBlock = uiBlockController.SetState(UIBlockState.Blocked);

			IslandInfo islandInfo = MetaplayClient.PlayerModel.GameConfig.Islands[islandId];
			await cameraControls.PanToIslandAsync(islandInfo, ct);

			if (!IsIslandUnlocked(islandId)) {
				return;
			}

			await UniTask.WhenAll(
				cloudCurtainsPresenter.Close(ct),
				cameraControls.ZoomInToIslandAsync(islandInfo, true, ct)
			);

			cameraControls.SetActive(false);

			applicationInfo.ActiveIsland.Value = islandId;

			mergeBoardRoot.gameObject.SetActive(true);
			mergeBoardRoot.SetupBoardAsync(applicationInfo.ActiveIsland.Value).Forget();

			Background.SetActive(true);

			// Wait for couple frames so the island has done heavy initialization logic
			await UniTask.DelayFrame(3, cancellationToken: ct);

			await cloudCurtainsPresenter.Open(ct);
		}

		private void EnterIslandInstant(IslandTypeId islandId) {
			cameraControls.SetActive(false);

			if (!IsIslandUnlocked(islandId)) {
				cameraControls.SetActive(true);
				return;
			}

			applicationInfo.ActiveIsland.Value = islandId;

			mergeBoardRoot.gameObject.SetActive(true);
			mergeBoardRoot.SetupBoardAsync(applicationInfo.ActiveIsland.Value).Forget();

			Background.SetActive(true);
		}

		private void OnMapControlsZoomedCloseToIsland(MapControlsZoomedCloseToIslandSignal signal) {
			// TODO: Provide a cancellation token that represents the controllers lifetime
			EnterIslandAsync(signal.Island.Type, default).Forget();
		}

		private void OnIslandFocused(IslandFocusedSignal signal) {
			IslandInfo islandInfo = MetaplayClient.PlayerModel.GameConfig.Islands[signal.Island];
			// TODO: Provide a cancellation token that represents the controllers lifetime
			cameraControls.FocusIslandAsync(islandInfo, false, CancellationToken.None).Forget();
		}

		private void OnIslandHighlighted(IslandHighlightedSignal signal) {
			HighlightIsland(signal.Island).Forget();
		}

		public async UniTask HighlightIsland(IslandTypeId island) {
			IslandInfo islandInfo = MetaplayClient.PlayerModel.GameConfig.Islands[island];
			// TODO: Add cancellation token
			await cameraControls.FocusIslandAsync(islandInfo, false, CancellationToken.None);
			triggerQueue.EnqueueAction(new HighlightElementTriggerAction($"{island.Value}"));
		}

		private void OnIslandPointed(IslandPointedSignal signal) {
			IslandInfo islandInfo = MetaplayClient.PlayerModel.GameConfig.Islands[signal.Island];
			// TODO: Add cancellation token
			cameraControls.FocusIslandAsync(islandInfo, false, CancellationToken.None).Forget();
		}

		private bool IsIslandUnlocked(IslandTypeId island) {
			var islandModel = MetaplayClient.PlayerModel.Islands[island];
			return islandModel.State == IslandState.Open;
		}

		public async UniTask GoToIslandAsync(IslandTypeId islandTypeId, CancellationToken ct) {
			if (applicationInfo.ActiveIsland.Value != islandTypeId) {
				if (applicationInfo.ActiveIsland.Value != null) {
					await LeaveIsland(ct);
				}

				await UniTask.WhenAll(
					EnterIslandAsync(islandTypeId, ct),
					WaitEnteredIslandInitializedAsync()
				);
			}

			async UniTask WaitEnteredIslandInitializedAsync() {
				UniTaskCompletionSource tcs = new();
				using (ct.Register(() => tcs.TrySetCanceled())) {
					void Handler(EnteredIslandSignal _) {
						tcs.TrySetResult();
					}

					signalBus.Subscribe<EnteredIslandSignal>(Handler);
					await tcs.Task;
					signalBus.Unsubscribe<EnteredIslandSignal>(Handler);
				}
			}
		}
	}
}
