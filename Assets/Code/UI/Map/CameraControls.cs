using System;
using System.Threading;
using Code.UI.Application;
using Code.UI.Map.Signals;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Code.UI.Map {
	/// <summary>
	/// The camera controls make use of a Unity <see cref="ScrollRect"/> to adjust the camera position based on the
	/// touch input of the user. The scroll content size is defined by the overall world map size and the zoom factor.
	/// The scroll rect covers the whole screen capturing standard scroll input by the user and translates this to a
	/// camera position. This way the smooth and elastic touch-scroll like in any other Unity UI is achieved.
	///
	/// The zoom behaviour requires additional calculations which are done in this class as well since there is no
	/// default implementation for pinch zoom in Unity UI.
	/// </summary>
	public class CameraControls : MonoBehaviour, IInitializable {
		private Vector3 panOrigin;

		[SerializeField] private Camera Camera;
		[SerializeField] private Transform Rig;
		[SerializeField] private GameObject IslandOverlay;
		[SerializeField] private ScrollRect ScrollRect;
		[SerializeField] private RectTransform ScrollContentArea;
		[SerializeField] private RectTransform CanvasRectTransform;

		[Header("Configuration"), SerializeField] private float AdditionalMapSpaceUnits = 50;
		[SerializeField] private float MinVisibleVerticalUnits = 10.0f;
		[SerializeField] private float EnterIslandVisibleVerticalUnits = 15.0f;
		[SerializeField] private float FocusIslandVisibleVerticalUnits = 20.0f;
		[SerializeField] private float MaxVisibleVerticalUnits = 100.0f;
		[SerializeField] private float PinchZoomSpeedFactor = 5000.0f;
		[SerializeField] private float EnterIslandTriggerDistance = 5.0f;

		[Header("Animation Speeds"), SerializeField] private float FocusIslandMoveDurationSeconds = 0.3f;
		[SerializeField] private float FocusIslandZoomDelaySeconds = 0.15f;
		[SerializeField] private float FocusIslandZoomDurationSeconds = 0.3f;
		[SerializeField] private float LeaveIslandZoomDelaySeconds = 0.15f;
		[SerializeField] private float LeaveIslandZoomDurationSeconds = 0.3f;

		[Inject] private SignalBus signalBus;
		[Inject] private Canvas canvas;
		[Inject] private IFrameRateController frameRateController;
		[Inject] private IWorldMapBehaviour worldMapBehaviour;

		private bool isSetup;
		private bool isCameraUpdateRequired;
		private bool isPinching;
		private bool isPastFocusIslandZoomFactor;
		private float pinchStartDistance;
		private Vector2 pinchStartTouchCenter;
		private Vector2 pinchStartScrollContentSizeDelta;
		private Vector3 pinchStartScrollContentAnchoredPosition;
		private Rect worldRect;
		private int inputLockCounter;

		// NOTE: Zoom factor 1 means the whole world is visible.
		private float MaxZoomFactor => isSetup ? worldRect.width / MinVisibleVerticalUnits : 1;
		private float EnterIslandZoomFactor => isSetup ? worldRect.width / EnterIslandVisibleVerticalUnits : 1;
		private float FocusIslandZoomFactor => isSetup ? worldRect.width / FocusIslandVisibleVerticalUnits : 1;
		private float MinZoomFactor => isSetup ? worldRect.width / MaxVisibleVerticalUnits : 1;

		protected void Awake() {
			Camera = Camera ? Camera : Camera.main;
			ScrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
		}

		protected void OnEnable() {
			MaintainHighFramerate(gameObject.GetCancellationTokenOnDestroy()).Forget();
		}

		private async UniTask MaintainHighFramerate(CancellationToken ct) {
			using (frameRateController.RequestHighFPS()) {
				await UniTask.WaitWhile(() => enabled, cancellationToken: ct);
			}
		}

		public void Initialize() {
			foreach (IslandInfo island in MetaplayClient.PlayerModel.GameConfig.Islands.Values) {
				worldRect.xMin = Mathf.Min(worldRect.xMin, island.X);
				worldRect.xMax = Mathf.Max(worldRect.xMax, island.X);
				worldRect.yMin = Mathf.Min(worldRect.yMin, island.Y);
				worldRect.yMax = Mathf.Max(worldRect.yMax, island.Y);
			}

			// Add additional margin around the area defined by the island positions.

			worldRect.xMin -= AdditionalMapSpaceUnits;
			worldRect.xMax += AdditionalMapSpaceUnits;
			worldRect.yMin -= AdditionalMapSpaceUnits;
			worldRect.yMax += AdditionalMapSpaceUnits;
			
			worldMapBehaviour.SetWorldSize(worldRect);

			Debug.Log($"World rect was {worldRect}");
			isSetup = true;
		}

		private void RequireCameraUpdate() => isCameraUpdateRequired = true;

		private void OnScrollRectValueChanged(Vector2 scrollValue) {
			RequireCameraUpdate();
		}

		protected void Update() {
			if (!HasAnyInputLock) {
				PinchZoom();
			}

			// Using the flag to update the camera on Update, so depending systems can have a guaranteed updated
			// camera position in LateUpdate
			if (isCameraUpdateRequired) {
				UpdateCameraFromScrollArea();
				isCameraUpdateRequired = false;
			}

			void UpdateCameraFromScrollArea() {
				if (!isSetup) {
					return;
				}

				float screenScrollSize = ScrollContentArea.sizeDelta.y;
				float screenViewSize = CanvasRectTransform.sizeDelta.y;
				float zoomFactor = screenScrollSize / screenViewSize;
				Vector2 worldViewSize = worldRect.size / zoomFactor;

				Camera.orthographicSize = worldViewSize.y / 2;

				// NOTE: The scroll area is not the full world size.
				// We need to remove one viewport size in world coordinates from the scroll area.
				// This keeps the viewport inside the scrollable area in the world.
				Vector2 worldScrollSize = worldRect.size - worldViewSize;
				Vector2 cameraPosition =
					worldRect.min + worldViewSize / 2 + worldScrollSize * ScrollRect.normalizedPosition;

				Rig.position = new Vector3(
					cameraPosition.x,
					0,
					cameraPosition.y
				);

				worldMapBehaviour.SetViewRectInWorld(
					new Rect {
						size = worldViewSize,
						center = cameraPosition
					}
				);
			}
		}

		private void PinchZoom() {
			#if UNITY_EDITOR
			// Small cheat to test simple multitouch zoom in the editor:
			// Hold Z and move the mouse.
			bool hasMultipleTouches = Input.GetKey(KeyCode.Z);
			Vector2 touch0Position = Input.mousePosition;
			// The second touch is the first touch point-mirrored from the screen middle
			Vector2 screenMiddle = new(Screen.width / 2.0f, Screen.height / 2.0f);
			Vector2 touch1Position = screenMiddle; // + (screenMiddle - touch0Position); // for keeping it centered
			#else
			bool hasMultipleTouches = Input.touchCount >= 2;
			Vector2 touch0Position = GetTouchOrZero(0);
			Vector2 touch1Position = GetTouchOrZero(1);
			static Vector2 GetTouchOrZero(int index) =>
				Input.touchCount > index ? Input.GetTouch(index).position : Vector2.zero;
			#endif

			if (hasMultipleTouches) {
				if (!isPinching) {
					isPinching = true;
					ScrollRect.enabled = false;
					ScrollContentArea.DOKill();

					pinchStartDistance = Vector2.Distance(touch0Position, touch1Position);
					pinchStartTouchCenter = Vector2.Lerp(touch0Position, touch1Position, 0.5f);
					pinchStartScrollContentSizeDelta = ScrollContentArea.sizeDelta;
					pinchStartScrollContentAnchoredPosition = ScrollContentArea.anchoredPosition;
				}

				float currentPinchDistance = Vector2.Distance(touch0Position, touch1Position);
				float pinchDistanceDelta = currentPinchDistance - pinchStartDistance;
				float pinchScreenRatio = pinchDistanceDelta / Screen.width;
				float delta = pinchScreenRatio * PinchZoomSpeedFactor;

				Vector2 targetSize = pinchStartScrollContentSizeDelta + Vector2.one * delta;
				if (targetSize.x > CanvasRectTransform.sizeDelta.x * MaxZoomFactor) {
					targetSize = GetScrollContentSizeForZoomFactor(MaxZoomFactor);
					delta = targetSize.x - pinchStartScrollContentSizeDelta.x;
				} else if (targetSize.x < CanvasRectTransform.sizeDelta.x * MinZoomFactor) {
					targetSize = GetScrollContentSizeForZoomFactor(MinZoomFactor);
					delta = targetSize.x - pinchStartScrollContentSizeDelta.x;
				}

				if (targetSize.x > CanvasRectTransform.sizeDelta.x * EnterIslandZoomFactor) {
					if (!isPastFocusIslandZoomFactor) {
						Vector3 rigPosition3D = Rig.position;
						Vector2 rigPosition2D = new(rigPosition3D.x, rigPosition3D.z);
						foreach (IslandInfo island in MetaplayClient.PlayerModel.GameConfig.Islands.Values) {
							if ((new Vector2(island.X, island.Y) - rigPosition2D).magnitude <
								EnterIslandTriggerDistance) {
								signalBus.TryFire(new MapControlsZoomedCloseToIslandSignal(island));
								break;
							}
						}

						isPastFocusIslandZoomFactor = true;
					}
				} else {
					isPastFocusIslandZoomFactor = false;
				}

				// To improve intuitive zooming the scroll content areas position should be adjusted as well
				// so the player can zoom into any point on the screen.
				Vector2 pinchStartTouchCenterInCanvas = pinchStartTouchCenter / canvas.scaleFactor;
				Vector3 adjustment = CalculatePositionAdjustmentForSizeChange(delta, pinchStartTouchCenterInCanvas);
				ScrollContentArea.anchoredPosition = pinchStartScrollContentAnchoredPosition - adjustment;

				ScrollContentArea.sizeDelta = targetSize;

				RequireCameraUpdate();
			} else if (isPinching) {
				isPinching = false;
				ScrollRect.enabled = !HasAnyInputLock;
			}
		}

		public UniTask FocusIslandAsync(IslandTypeId islandTypeId, bool entering, CancellationToken ct) {
			IslandInfo islandInfo = MetaplayClient.PlayerModel.GameConfig.Islands[islandTypeId];
			return FocusIslandAsync(islandInfo, entering, ct);
		}

		public async UniTask FocusIslandAsync(IslandInfo island, bool entering, CancellationToken ct) {
			await PanToIslandAsync(island, ct);
			await ZoomInToIslandAsync(island, entering, ct);
		}

		public async UniTask PanToIslandAsync(IslandInfo island, CancellationToken ct) {
			using InputLock _ = new(this);
			Vector2 normalizedScrollPosition = CalculateNormalizedScrollPositionForIsland(island);

			bool needsToPan = ScrollRect.normalizedPosition != normalizedScrollPosition;
			if (needsToPan) {
				await DOTween.Sequence()
					.OnUpdate(RequireCameraUpdate)
					.Join(ScrollRect.DONormalizedPos(normalizedScrollPosition, FocusIslandMoveDurationSeconds))
					.ToUniTask(cancellationToken: ct);
			} else {
				ScrollRect.normalizedPosition = normalizedScrollPosition;
			}
		}

		public async UniTask ZoomInToIslandAsync(IslandInfo island, bool entering, CancellationToken ct) {
			using InputLock _ = new(this);
			Vector2 normalizedScrollPosition = CalculateNormalizedScrollPositionForIsland(island);
			ScrollRect.normalizedPosition = normalizedScrollPosition;
			Vector2 targetSize =
				GetScrollContentSizeForZoomFactor(entering ? MaxZoomFactor : FocusIslandZoomFactor);
			float delta = targetSize.x - ScrollContentArea.sizeDelta.x;

			Vector2 screenCenter = new(Screen.width / 2.0f, Screen.height / 2.0f);
			Vector2 screenCenterInCanvas = screenCenter / canvas.scaleFactor;
			Vector2 adjustment = CalculatePositionAdjustmentForSizeChange(delta, screenCenterInCanvas);

			await DOTween.Sequence()
				.OnUpdate(RequireCameraUpdate)
				.AppendInterval(FocusIslandZoomDelaySeconds)
				.Append(ScrollContentArea.DOSizeDelta(targetSize, FocusIslandZoomDurationSeconds))
				.Join(
					ScrollContentArea.DOAnchorPos(
						ScrollContentArea.anchoredPosition - adjustment,
						FocusIslandZoomDurationSeconds
					)
				)
				.ToUniTask(cancellationToken: ct);
		}

		public void FocusIsland(IslandInfo island) {
			if (!isSetup) {
				return;
			}

			Vector2 normalizedScrollPosition = CalculateNormalizedScrollPositionForIsland(island);
			ScrollRect.normalizedPosition = normalizedScrollPosition;
			RequireCameraUpdate();
		}

		public async UniTask LeaveIsland(IslandInfo island, CancellationToken ct) {
			using InputLock _ = new(this);
			FocusIsland(island);
			Vector2 targetSize = GetScrollContentSizeForZoomFactor(FocusIslandZoomFactor);
			float delta = targetSize.x - ScrollContentArea.sizeDelta.x;
			Vector2 screenCenter = new(Screen.width / 2.0f, Screen.height / 2.0f);
			Vector2 screenCenterInCanvas = screenCenter / canvas.scaleFactor;
			Vector2 adjustment = CalculatePositionAdjustmentForSizeChange(delta, screenCenterInCanvas);
			await DOTween.Sequence()
				.OnUpdate(RequireCameraUpdate)
				.AppendInterval(LeaveIslandZoomDelaySeconds)
				.Append(ScrollContentArea.DOSizeDelta(targetSize, LeaveIslandZoomDurationSeconds))
				.Join(
					ScrollContentArea.DOAnchorPos(
						ScrollContentArea.anchoredPosition - adjustment,
						LeaveIslandZoomDurationSeconds
					)
				)
				.ToUniTask(cancellationToken: ct);
		}

		private Vector2 CalculateNormalizedScrollPositionForIsland(IslandInfo island) {
			Vector2 scrollContentAreaSize = ScrollContentArea.sizeDelta;
			Vector2 canvasSize = CanvasRectTransform.sizeDelta;
			Vector2 relativeViewPortSize = canvasSize / scrollContentAreaSize;
			Vector2 viewPortSizeInWorld = relativeViewPortSize * worldRect.size;
			Vector2 halfViewPortSizeInWorld = viewPortSizeInWorld / 2;
			Rect scrollableWorldRect = new(
				worldRect.min + halfViewPortSizeInWorld,
				worldRect.size - viewPortSizeInWorld);
			Vector2 islandWorldPosition = new(island.X, island.Y);
			Vector2 transformedIslandPosition = islandWorldPosition - scrollableWorldRect.min;
			Vector2 normalizedIslandPosition = transformedIslandPosition / scrollableWorldRect.size;
			return normalizedIslandPosition;
		}

		private Vector2 CalculatePositionAdjustmentForSizeChange(float delta, Vector2 pivotInCanvas) {
			Vector2 scrollContentAreaPosition = ScrollContentArea.anchoredPosition;
			Vector2 pivotInScrollContent = pivotInCanvas - scrollContentAreaPosition;
			Vector2 normalizedPivotInScrollContent =
				pivotInScrollContent / ScrollContentArea.sizeDelta;
			return delta * normalizedPivotInScrollContent;
		}

		private Vector2 GetScrollContentSizeForZoomFactor(float zoomFactor) {
			Vector2 canvasSizeDelta = CanvasRectTransform.sizeDelta;
			Vector2 scaledCanvasSizeDelta = canvasSizeDelta * zoomFactor;
			float additionalDeltaX = scaledCanvasSizeDelta.x - canvasSizeDelta.x;
			// Adjust the y component for a uniform zoom in both axis
			return new Vector2(scaledCanvasSizeDelta.x, canvasSizeDelta.y + additionalDeltaX);
		}

		// HACK: This is a quick solution to block input in certain conditions
		// It is not ideal that the scroll rect state is depending on two systems: zooming and input blocks
		// TODO: Add a global input block controller that allows adding temporary blocks
		private bool HasAnyInputLock => inputLockCounter > 0;

		private class InputLock : IDisposable {
			private readonly CameraControls Controls;

			public InputLock(CameraControls controls) {
				Controls = controls;
				controls.inputLockCounter++;
				controls.ScrollRect.enabled = false;
			}

			public void Dispose() {
				Controls.inputLockCounter--;
				if (Controls.inputLockCounter <= 0) {
					Controls.ScrollRect.enabled = !Controls.isPinching;
				}
			}
		}

		public void SetActive(bool active) {
			enabled = active;
			ScrollRect.gameObject.SetActive(active);
			IslandOverlay.SetActive(active);
		}
	}
}
