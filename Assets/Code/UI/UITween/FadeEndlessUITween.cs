using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	[RequireComponent(typeof(CanvasGroup))]
	public class FadeEndlessUITween : EndlessUITween {
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private float StartAlpha = 0;
		[SerializeField] private float EndAlpha = 1;
		[SerializeField] private Ease Ease = Ease.InOutQuad;

		protected override Tween CreateTween() {
			return GetComponent<CanvasGroup>()
				.DOFade(EndAlpha, DurationInSeconds)
				.From(StartAlpha)
				.SetEase(Ease);
		}
	}
}
