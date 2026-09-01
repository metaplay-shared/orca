using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	public class ScaleChildrenInTween : MonoBehaviour {
		[SerializeField] private float DelayInSeconds = 0f;
		[SerializeField] private float Interval = 0f;
		[SerializeField] private float StartScaleFactor = 0f;
		[SerializeField] private float TargetScaleFactor = 1f;
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private Ease Ease = Ease.OutBack;

		private Tween tween;

		private void OnEnable() {
			ExecuteTween();
		}

		private void OnDisable() {
			DOTween.Kill(tween);
		}

		private void ExecuteTween() {
			var sequence = DOTween.Sequence();
			sequence.AppendInterval(DelayInSeconds);

			var first = true;
			foreach (RectTransform rectTransform in transform) {
				if (!rectTransform.gameObject.activeSelf) {
					continue;
				}

				rectTransform.localScale = Vector3.one * StartScaleFactor;

				if (!first) {
					sequence.AppendInterval(Interval);
				}

				sequence.AppendCallback(
					() =>
						rectTransform
							.DOScale(TargetScaleFactor, DurationInSeconds)
							.From(StartScaleFactor)
							.SetEase(Ease)
				);
				first = false;
			}

			tween = sequence;
		}
	}
}
