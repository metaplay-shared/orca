using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	[RequireComponent(typeof(RectTransform))]
	public class JumpInUITween : DelayedUITween {
		[SerializeField] private float JumpPower = 1f;
		[SerializeField] private int JumpCount = 1;
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private Ease Ease = Ease.Linear;

		protected override Tween CreateTween() {
			return GetComponent<RectTransform>()
				.DOJumpAnchorPos(Vector2.zero, JumpPower, JumpCount, DurationInSeconds)
				.SetEase(Ease);
		}
	}
}
