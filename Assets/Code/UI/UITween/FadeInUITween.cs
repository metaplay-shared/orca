using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	[RequireComponent(typeof(CanvasGroup))]
	public class FadeInUITween : DelayedUITween {
		[SerializeField] private float StartAlpha;
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private Ease Ease = Ease.Linear;

		protected override Tween CreateTween() {
			return GetComponent<CanvasGroup>()
				.DOFade(StartAlpha, DurationInSeconds)
				.From()
				.SetEase(Ease);
		}
	}
}
