using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	[RequireComponent(typeof(RectTransform))]
	public class RotateEndlessUITween : EndlessUITween {
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private float TargetRotation = 360;
		[SerializeField] private Ease Ease = Ease.InOutQuad;

		protected override Tween CreateTween() {
			return transform
				.DORotate(new Vector3(0, 0, TargetRotation), DurationInSeconds, RotateMode.FastBeyond360)
				.SetRelative(true)
				.SetEase(Ease);
		}
	}
}
