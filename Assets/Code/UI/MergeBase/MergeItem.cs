using System;
using System.Threading;
using Code.UI.Application;
using Code.UI.AssetManagement;
using Code.UI.Merge.AddOns;
using Code.UI.MergeBase.Signals;
using Code.UI.Tasks.Hero;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.MergeBase {
	public class MergeItem : MonoBehaviour {
		[SerializeField] private Image ConverterFillStatus;
		[SerializeField] private Image GroundArt;
		[SerializeField] private Image Art;
		[SerializeField] private Image HiddenLockBack;
		[SerializeField] private Image PartialLockBack;
		[SerializeField] private Image HiddenLockFront;
		[SerializeField] private Image PartialLockFront;
		[SerializeField] private Image UsedIndicator;
		[SerializeField] private GameObject MaxLevelIndicator;

		[SerializeField] private Sprite LargeHiddenLockBack;
		[SerializeField] private Sprite LargePartialLockBack;
		[SerializeField] private Sprite LargeHiddenLockFront;
		[SerializeField] private Sprite LargePartialLockFront;

		public const float MERGE_BOUNCE_HIGH = 1.0f;
		public const float MERGE_BOUNCE_MEDIUM = 0.5f;
		public const float MERGE_BOUNCE_LOW = 0.25f;

		public GameObject Handle => gameObject;

		public IMergeItemModelAdapter Adapter;

		[Inject] private SignalBus signalBus;
		[Inject] private AddressableManager addressableManager;
		[Inject] private DiContainer container;
		[Inject] private IFrameRateController frameRateController;

		private ItemAddOn[] addOns;

		private Tween tween;
		private Transform originalParent;
		private bool isDragging;

		public int LastKnownX { get; private set; }
		public int LastKnownY { get; private set; }

		public bool Interactable { get; private set; }

		public Sprite ItemSprite => Art.sprite;

		public void Setup(IMergeItemModelAdapter adapter, RectTransform itemOverlayLayer, CancellationToken ct) {
			Adapter = adapter;

			if (Adapter.Width > 1) {
				HiddenLockBack.sprite = LargeHiddenLockBack;
				PartialLockBack.sprite = LargePartialLockBack;
				HiddenLockFront.sprite = LargeHiddenLockFront;
				PartialLockFront.sprite = LargePartialLockFront;
			}

			try {
				LoadSprite(adapter);
			} catch (Exception e) {
				Debug.LogException(e);
			}

			var rt = GetComponent<RectTransform>();
			rt.sizeDelta = new Vector2(
				MergeBoardRoot.TILE_WIDTH * adapter.Width,
				MergeBoardRoot.TILE_HEIGHT * adapter.Height
			);
			rt.pivot = new Vector2(0.5f / adapter.Width, 0.5f / adapter.Height);

			UpdateLastKnownPosition();

			// ToDo: Move this to addon
			if (adapter.IsHeroItemTarget) {
				HeroItemFlightTarget flightTarget = (HeroItemFlightTarget)container.InstantiateComponent(typeof(HeroItemFlightTarget), gameObject);
				//HeroItemFlightTarget flightTarget = gameObject.AddComponent<HeroItemFlightTarget>();
				flightTarget.Type = adapter.Type;
			}

			foreach (var addOn in addOns) {
				addOn.Setup(adapter, itemOverlayLayer);
			}

			UpdateState();

			Interactable = true;
		}

		private void LoadSprite(IMergeItemModelAdapter adapter) {
			Sprite chainSprite = addressableManager.GetItemIcon(adapter.Type, adapter.Level);
			if (Adapter.IsBuilding && Adapter.Level == 1) {
				GroundArt.sprite = chainSprite;
				Art.gameObject.SetActive(false);
				GroundArt.gameObject.SetActive(true);
			} else {
				Art.sprite = chainSprite;
				Art.gameObject.SetActive(true);
				GroundArt.gameObject.SetActive(false);
			}
		}

		public async UniTask MoveToAsync(CancellationToken ct, MergeTile to, bool fadeIn) {
			foreach (var addOn in addOns) {
				if (addOn.IsActive) {
					addOn.OnBeginMove();
				}
			}

			tween?.Kill(true);
			var targetPosition = to.Handle.transform.position;
			var duration = Adapter.GetFlightTime(Vector3.Distance(transform.position, targetPosition));

			Sequence moveAndBounce = DOTween
				.Sequence()
				.Join(transform.DOMove(targetPosition, duration))
				.Join(DoBounceAsync(strength: MERGE_BOUNCE_LOW));

			if (fadeIn) {
				float alpha = Art.color.a;
				Art.color = new Color(Art.color.r, Art.color.g, Art.color.b, 0);
				moveAndBounce.Join(Art.DOFade(alpha, 0.5f));
			}

			UpdateLastKnownPosition();
			tween = moveAndBounce;

			using (frameRateController.RequestHighFPS()) {
				await moveAndBounce.AwaitForComplete(TweenCancelBehaviour.Complete, ct);
			}

			if (this != null && gameObject != null) {
				foreach (var addOn in addOns) {
					if (addOn.IsActive) {
						addOn.OnEndMove();
					}
				}
			}
		}

		private void Awake() {
			addOns = GetComponents<ItemAddOn>();
		}

		public void OnEnable() {
			signalBus.Subscribe<ItemCreatedSignal>(UpdateState);
			signalBus.Subscribe<ItemRemovedSignal>(UpdateState);
			signalBus.Subscribe<ItemStateChangedSignal>(OnItemStateChanged);
			signalBus.Subscribe<BuilderUsedSignal>(OnBuilderUsed);
			signalBus.Subscribe<MergeBoardStateChangedSignal>(UpdateState);

			foreach (var addOn in addOns) {
				if (addOn.IsActive) {
					addOn.OnItemEnabled();
				}
			}

			UpdateState();
		}

		private void OnDisable() {
			signalBus.Unsubscribe<ItemCreatedSignal>(UpdateState);
			signalBus.Unsubscribe<ItemRemovedSignal>(UpdateState);
			signalBus.Unsubscribe<ItemStateChangedSignal>(OnItemStateChanged);
			signalBus.Unsubscribe<BuilderUsedSignal>(OnBuilderUsed);
			signalBus.Unsubscribe<MergeBoardStateChangedSignal>(UpdateState);

			foreach (var addOn in addOns) {
				if (addOn.IsActive) {
					addOn.OnItemDisabled();
				}
			}
		}

		private void UpdateLastKnownPosition() {
			LastKnownX = Adapter.X;
			LastKnownY = Adapter.Y;
		}

		public async UniTask BounceAsync(CancellationToken ct, float duration = 0.5f, float strength = 0.5f) {
			if (isDragging) {
				return;
			}
			tween?.Kill(true);
			tween = DoBounceAsync(duration, strength);
			await tween.AwaitForComplete(TweenCancelBehaviour.Complete, ct);
		}

		private Tween DoBounceAsync(float duration = 0.5f, float strength = 0.5f) {
			transform.localScale = Vector3.one;
			return transform.DOPunchScale(
				new Vector3(
					strength,
					strength,
					strength
				),
				duration,
				1
			);
		}

		public async UniTask NudgeAsync(CancellationToken ct, Vector3 direction, float strength = 10) {
			if (isDragging) {
				return;
			}
			tween?.Kill(true);
			tween = transform.DOPunchPosition(direction * strength, 0.5f, 1);
			await tween.AwaitForComplete(TweenCancelBehaviour.Complete, ct);
		}

		private void OnItemStateChanged(ItemStateChangedSignal signal) {
			if (signal.Item == Adapter.Id) {
				UpdateState();

				if (Adapter.BuildState == ItemBuildState.PendingComplete) {
					Adapter.AcknowledgeBuilding().Forget();
				}
			}
		}

		private void UpdateState() {
			if (Adapter == null) {
				return;
			}

			ConverterFillStatus.fillAmount = Adapter.Progression;

			bool hidden = Adapter.State == ItemState.Hidden || Adapter.State == ItemState.PartiallyVisible;
			HiddenLockBack.gameObject.SetActive(hidden);
			HiddenLockFront.gameObject.SetActive(hidden);
			PartialLockBack.gameObject.SetActive(Adapter.State == ItemState.FreeForMerge);
			PartialLockFront.gameObject.SetActive(Adapter.State == ItemState.FreeForMerge);
			UsedIndicator.gameObject.SetActive(Adapter.IsUsedInTask);
			MaxLevelIndicator.SetActive(
				Adapter.IsMaxLevel &&
				Adapter.Level > 1 &&
				!Adapter.IsBuilding &&
				Adapter.Width == 1 &&
				!Adapter.UnderLockArea &&
				!hidden
			);

			foreach (var addOn in addOns) {
				addOn.OnStateChanged();
			}
		}

		public async UniTask DestroySelfAsync(CancellationToken ct) {
			tween?.Kill(true);
			Interactable = false;
			foreach (var addOn in addOns) {
				addOn.OnDestroySelf();
			}

			await UniTask.DelayFrame(1, cancellationToken: ct);

			Sequence bounceAndFade = DOTween
				.Sequence()
				.Join(DoBounceAsync(0.25f))
				.Join(Art.DOFade(0, 0.25f));

			await bounceAndFade;
			Destroy(gameObject);
		}

		public void StopAnimations() {
			tween?.Kill(true);
		}

		public void OnSelected(bool created) {
			if (created) {
				BounceAsync(default).Forget();
			} else {
				BounceAsync(default, strength: MERGE_BOUNCE_LOW / Adapter.Height, duration: 0.25f).Forget();
			}

			foreach (var addOn in addOns) {
				if (addOn.IsActive) {
					addOn.OnSelected();
				}
			}

			Adapter.Select();
		}

		public void OnDeselected() {
			foreach (var addOn in addOns) {
				if (addOn.IsActive) {
					addOn.OnDeselected();
				}
			}
		}

		public void OnBeginDrag() {
			isDragging = true;
			tween?.Kill(true);
			foreach (var addOn in addOns) {
				if (addOn.IsActive) {
					addOn.OnBeginDrag();
				}
			}

			MoveToTopLayer();
		}

		public void OnEndDrag() {
			isDragging = false;
			foreach (var addOn in addOns) {
				if (addOn.IsActive) {
					addOn.OnEndDrag();
				}
			}

			ReturnFromTopLayer();
		}

		public void OnHoverMergeTarget(bool isHoveringMergeTarget, MergeItem mergeItem) {
			foreach (var addOn in addOns) {
				if (addOn.IsActive) {
					addOn.OnHoverMergeTarget(isHoveringMergeTarget, mergeItem);
				}
			}
		}

		private void MoveToTopLayer() {
			originalParent = transform.parent;
			GameObject topLayer = GameObject.Find("TopLayer");
			transform.SetParent(topLayer.transform, true);
			transform.SetAsLastSibling();
		}

		private void OnBuilderUsed(BuilderUsedSignal signal) {
			if (signal.Duration == 0 || signal.Item != Adapter.Id) {
				return;
			}

			tween?.Kill(true);
			int duration = Math.Min(signal.Duration, 3);
			Sequence bounce = DOTween.Sequence();
			for (int i = 0; i < 2 * duration; i++) {
				bounce.Append(Art.transform.DOPunchScale(new Vector3(0, 0.25f, 0), 0.25f, 1));
				bounce.Append(Art.transform.DOPunchScale(new Vector3(0.25f, 0, 0), 0.25f, 1));
			}

			tween = bounce;
		}

		private void ReturnFromTopLayer() {
			transform.SetParent(originalParent);
		}

		public void Open() {
			foreach (var addOn in addOns) {
				if (addOn.IsActive) {
					addOn.OnOpen();
				}
			}

			Adapter.Open();
		}
	}
}
