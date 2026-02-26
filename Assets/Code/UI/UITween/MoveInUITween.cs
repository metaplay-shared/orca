using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	[RequireComponent(typeof(RectTransform))]
	public class MoveInUITween : DelayedUITween {
		[SerializeField] private Vector2 StartOffset;
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private Ease Ease = Ease.OutQuad;

		protected override Tween CreateTween() {
			return GetComponent<RectTransform>()
				.DOLocalMove(StartOffset, DurationInSeconds)
				.From(isRelative: true)
				.SetEase(Ease);
		}
	}
}
