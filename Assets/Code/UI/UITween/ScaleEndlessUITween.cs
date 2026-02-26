using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	[RequireComponent(typeof(RectTransform))]
	public class ScaleEndlessUITween : EndlessUITween {
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private Vector2 TargetScale = Vector2.one * 2;
		[SerializeField] private bool IsRelative = true;
		[SerializeField] private Ease Ease = Ease.InOutQuad;

		protected override Tween CreateTween() {
			return transform
				.DOScale(TargetScale, DurationInSeconds)
				.SetRelative(IsRelative)
				.SetEase(Ease);
		}
	}
}
