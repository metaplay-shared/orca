using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	public class PositionShakeOnEnableUITween : OnEnableTween {
		[SerializeField] private float DurationInSeconds;
		[SerializeField] private Vector3 Strength;
		[SerializeField] private int Vibrato = 10;
		[SerializeField] private float Randomness = 90f;
		[SerializeField] private bool Snapping = false;
		[SerializeField] private bool FadeOut = true;

		protected override Tween CreateTween() {
			DOTween.Complete(transform);
			return transform.DOShakePosition(
				DurationInSeconds,
				Strength,
				Vibrato,
				Randomness,
				Snapping,
				FadeOut
			);
		}
	}
}
