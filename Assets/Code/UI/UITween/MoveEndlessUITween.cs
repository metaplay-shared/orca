using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	[RequireComponent(typeof(RectTransform))]
	public class MoveEndlessUITween : EndlessUITween {
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private Vector2 Offset;
		[SerializeField] private Ease Ease = Ease.InOutQuad;

		protected override Tween CreateTween() {
			return GetComponent<RectTransform>()
				.DOLocalMove(Offset, DurationInSeconds)
				.SetRelative(true)
				.SetEase(Ease);
		}
	}
}
