using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	[RequireComponent(typeof(CanvasGroup))]
	public class FadeInOnEnableUITween : OnEnableTween {
		[SerializeField] private CanvasGroup CanvasGroup;
		[SerializeField] private float StartAlpha;
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private Ease Ease = Ease.Linear;

		private void Reset() {
			CanvasGroup = GetComponent<CanvasGroup>();
		}

		protected override Tween CreateTween() {
			return CanvasGroup
				.DOFade(StartAlpha, DurationInSeconds)
				.From()
				.SetEase(Ease);
		}
	}
}
