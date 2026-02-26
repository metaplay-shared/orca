using System.Threading;
using Code.UI.Application;
using Code.UI.Core.AndroidBackButton;
using Code.UI.Core.UIBlock;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Orca.Common;
using UnityEngine;

namespace Code.UI.Core {
	public interface IUIRoot<in TUIHandle> : IUIStackItem
		where TUIHandle : class, IUIHandle {
		CancellationToken GetCancellationTokenOnDestroy();

		UniTask Present(
			TUIHandle handle,
			IUIBlockController uiBlockController,
			IFrameRateController frameRateController,
			CancellationToken ct
		);
	}

	public abstract class UIRootBase<TUIHandle> : MonoBehaviour, IUIRoot<TUIHandle>
		where TUIHandle : class, IUIHandle {
		[SerializeField] protected CanvasGroup Background;
		[SerializeField] protected CanvasGroup Content;

		protected CancellationToken UICancellationToken { get; private set; }
		protected TUIHandle UIHandle { get; private set; }

		private Option<Tween> showAnimation;
		private Option<Tween> hideAnimation;

		public CancellationToken GetCancellationTokenOnDestroy() {
			return gameObject.GetCancellationTokenOnDestroy();
		}

		async UniTask IUIRoot<TUIHandle>.Present(
			TUIHandle handle,
			IUIBlockController uiBlockController,
			IFrameRateController frameRateController,
			CancellationToken ct
		) {
			UICancellationToken = ct;
			UIHandle = handle;

			CreateAnimations();

			handle.SetStarted();

			Init();

			using (frameRateController.RequestHighFPS()) {
				await Show(ct);
			}

			handle.SetEnterIdle();

			using (uiBlockController.SetState(UIBlockState.Unblocked)) {
				await Idle(ct);
			}

			handle.SetExitIdle();
			using (frameRateController.RequestHighFPS()) {
				await Hide(ct);
			}
			handle.SetComplete();
		}

		protected virtual void CreateAnimations() {
			hideAnimation = CreateHideAnimation();
			showAnimation = CreateShowAnimation();

			foreach (var tween in showAnimation) {
				tween.Pause();
			}

			foreach (var tween in hideAnimation) {
				tween.Pause();
			}
		}

		protected virtual Option<Tween> CreateShowAnimation() {
			var contentRectTransform = Content.GetComponent<RectTransform>();

			return DOTween.Sequence()
				.Join(
					contentRectTransform
						.DOAnchorPosY(endValue: -500, duration: 0.3f)
						.From(isRelative: true)
						.SetEase(Ease.OutBack)
				)
				.Join(
					Content
						.DOFade(endValue: 1, duration: 0.15f).From(fromValue: 0)
						.SetEase(Ease.InQuad)
				)
				.Join(
					Background
						.DOFade(endValue: 1, duration: 0.15f).From(fromValue: 0)
						.SetEase(Ease.InQuad)
				);
		}

		protected virtual Option<Tween> CreateHideAnimation() {
			var contentRectTransform = Content.GetComponent<RectTransform>();
			return DOTween.Sequence()
				.Join(
					contentRectTransform
						.DOAnchorPosY(endValue: -250, duration: 0.15f)
						.SetRelative(true)
						.SetEase(Ease.InQuad)
				)
				.Join(
					Content
						.DOFade(endValue: 0, duration: 0.15f)
						.SetEase(Ease.InQuad)
				)
				.Join(
					Background
						.DOFade(endValue: 0, duration: 0.15f)
						.SetEase(Ease.InQuad)
				);
		}

		protected abstract void Init();

		protected virtual async UniTask Show(CancellationToken ct) {
			// Yield one frame to avoid heavy initialisation affect the following show animation
			await UniTask.Yield();
			await PlayAnimation(showAnimation, ct);
		}

		protected abstract UniTask Idle(CancellationToken ct);

		protected virtual UniTask Hide(CancellationToken ct) => PlayAnimation(hideAnimation, ct);

		protected UniTask OnBackgroundClickAsync(CancellationToken ct) {
			return Background.GetAsyncPointerClickTrigger().OnPointerClickAsync(ct);
		}

		private static UniTask PlayAnimation(Option<Tween> tween, CancellationToken ct) {
			foreach (var anim in tween) {
				return anim.Play().WithCancellation(ct);
			}

			return UniTask.CompletedTask;
		}

		void IAndroidBackButtonHandler.HandleAndroidBackButtonPressed() {
			if (UIHandle.OnEnterIdle.Status == UniTaskStatus.Succeeded &&
				UIHandle.OnExitIdle.Status == UniTaskStatus.Pending) {
				HandleAndroidBackButtonPressed();
			}
		}

		protected abstract void HandleAndroidBackButtonPressed();
	}

	public abstract class UIRootWithResultBase<TUIHandle, TUIResult> : UIRootBase<TUIHandle>
		where TUIHandle : class, IUIHandleWithResult<TUIResult>
		where TUIResult : IUIResult {
		protected sealed override async UniTask Idle(CancellationToken ct) {
			var result = await IdleWithResult(ct);
			UIHandle.SetResult(result);
		}

		protected abstract UniTask<TUIResult> IdleWithResult(CancellationToken ct);
	}
}
