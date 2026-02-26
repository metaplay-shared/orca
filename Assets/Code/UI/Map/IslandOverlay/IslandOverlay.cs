using System.Threading;
using Code.UI.Core;
using Code.UI.InfoMessage.Signals;
using Code.UI.Island;
using Code.UI.Map.Signals;
using Code.UI.Merge.AddOns.MergeBoard.LockArea;
using Code.UI.Shop;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Map.IslandOverlay {
	public class IslandOverlay : MonoBehaviour {
		[SerializeField] private TMP_Text IslandNameText;
		[SerializeField] private GameObject LockContainer;
		[SerializeField] private GameObject OpenContainer;
		[SerializeField] private TMP_Text UnlockInfoText;
		[SerializeField] private Button UnlockButton;
		[SerializeField] private LockAreaDetails LevelAndHero;
		[SerializeField] private CanvasGroup CanvasGroup;
		[SerializeField] private GameObject Notification;
		[Header("Animation"),  SerializeField] private float FadeDuration = 0.5f;

		[Inject] private SignalBus signalBus;
		[Inject] private Canvas canvas;
		[Inject] private UIController uiController;
		[Inject] private IUIRootController uiRootController;
		[Inject] private CameraControls cameraControls;
		[Inject] private IIslandOverlayViewModel viewModel;

		private Island island;
		private RectTransform rectTransform;
		private new Camera camera;
		private bool isVisible;

		public RectTransform RectTransform => rectTransform;

		private void OnEnable() {
			viewModel.Update();
			Observable.Interval(TimeSpan.FromSeconds(1))
				.TakeUntilDisable(gameObject)
				.Subscribe(_ => viewModel.Update());
			signalBus.Subscribe<IslandStateChangedSignal>(OnIslandStateChanged);
			signalBus.Subscribe<ResourcesChangedSignal>(OnResourcesChanged);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<IslandStateChangedSignal>(OnIslandStateChanged);
			signalBus.Unsubscribe<ResourcesChangedSignal>(OnResourcesChanged);
		}

		private void Awake() {
			rectTransform = GetComponent<RectTransform>();
			camera = Camera.main;
			CanvasGroup.alpha = 0;
			viewModel.HasSomethingToDo.Subscribe(HandleHasSomethingToDoChanged).AddTo(gameObject);
		}

		public void Setup(Island targetIsland) {
			island = targetIsland;
			IslandNameText.text = targetIsland.Model.Info.Type.Localize();
			UpdateState();
		}

		protected void LateUpdate() {
			Vector3 screenPos = camera.WorldToScreenPoint(island.transform.position);
			Vector3 rectPos = screenPos / canvas.scaleFactor;
			rectTransform.anchoredPosition = rectPos;
			bool isInScreenRect = new Rect(0, 0, Screen.width, Screen.height).Contains(screenPos);
			if (isInScreenRect && !isVisible) {
				AppearAsync(gameObject.GetCancellationTokenOnDestroy()).Forget();
			} else if (!isInScreenRect && isVisible) {
				DisappearAsync(gameObject.GetCancellationTokenOnDestroy()).Forget();
			}
		}

		private async UniTask AppearAsync(CancellationToken ct) {
			isVisible = true;
			CanvasGroup.DOKill();
			await CanvasGroup
				.DOFade(1, FadeDuration)
				.ToUniTask(cancellationToken: ct);
		}

		private async UniTask DisappearAsync(CancellationToken ct) {
			isVisible = false;
			CanvasGroup.DOKill();
			await CanvasGroup
				.DOFade(0, FadeDuration)
				.ToUniTask(cancellationToken: ct);
		}

		public void OnClick() {
			uiController.EnterIslandAsync(island.Model.Info.Type, default).Forget();
		}

		private void OnResourcesChanged(ResourcesChangedSignal signal) {
			if (signal.ResourceType != CurrencyTypeId.IslandTokens) {
				return;
			}

			UpdateState();
		}

		private void OnIslandStateChanged(IslandStateChangedSignal signal) {
			if (signal.IslandTypeId != island.Model.Info.Type) {
				return;
			}

			UpdateState();
		}

		private void UpdateState() {
			if (island.Model.State == IslandState.Hidden) {
				LockContainer.SetActive(true);
				OpenContainer.SetActive(false);
				UnlockButton.gameObject.SetActive(false);
				LevelAndHero.gameObject.SetActive(true);
				LevelAndHero.Setup(island.Model.Info.PlayerLevel, island.Model.Info.Hero, island.Model.Info.HeroLevel);
			} else if (island.Model.State is IslandState.Revealing) {
				LockContainer.SetActive(false);
				OpenContainer.SetActive(false);
			} else if (island.Model.State == IslandState.Locked) {
				LockContainer.SetActive(true);
				OpenContainer.SetActive(false);
				LevelAndHero.gameObject.SetActive(false);
				UnlockButton.gameObject.SetActive(true);

				// TODO: Handle other currency types
				UnlockInfoText.text = Localizer.Localize("Info.TokensRequired", island.Model.Info.UnlockCost.Amount);
				UnlockButton.onClick.RemoveAllListeners();
				UnlockButton.onClick.AddListener(TryUnlockIsland);
			} else {
				LockContainer.SetActive(false);
				OpenContainer.SetActive(true);
			}
		}

		private void TryUnlockIsland() {
			if (island.Model.State != IslandState.Locked) {
				return;
			}

			if (MetaplayClient.PlayerModel.Wallet.EnoughCurrency(
				island.Model.Info.UnlockCost.Type,
				island.Model.Info.UnlockCost.Amount
			)) {
				cameraControls
					.FocusIslandAsync(island.Model.Info, false, CancellationToken.None)
					.Forget();
				uiRootController.ShowUI<UnlockIslandPopup, UnlockIslandPopupHandle>(
					new UnlockIslandPopupHandle(island.Model.Info.Type),
					CancellationToken.None
				).OnComplete.ContinueWith(
					() => island.Model.State == IslandState.Open
						? uiController.EnterIslandAsync(island.Model.Info.Type, CancellationToken.None)
						: UniTask.CompletedTask
				).Forget();
				return;
			}

			if (island.Model.Info.UnlockCost.Type == CurrencyTypeId.IslandTokens) {
				signalBus.Fire(
					new InfoMessageSignal(Localizer.Localize("Info.NotEnoughIslandTokens"))
				);
			} else {
				uiRootController.ShowUI<ShopUIRoot, ShopUIHandle>(
					new ShopUIHandle(new ShopUIHandle.ShopNavigationPayload()),
					CancellationToken.None
				);
			}
		}

		private void HandleHasSomethingToDoChanged(bool hasSomethingToDo) {
			Notification.SetActive(hasSomethingToDo);
		}
	}
}
