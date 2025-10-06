using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	[RequireComponent(typeof(RectTransform))]
	public class ScaleInUITween : DelayedUITween {
		[SerializeField] private float StartScaleFactor = 0;
		[SerializeField] private float DurationInSeconds = 0.3f;
		[SerializeField] private Ease Ease = Ease.OutBack;

		protected override Tween CreateTween() {
			var rectTransform = GetComponent<RectTransform>();
			return rectTransform.DOScale(StartScaleFactor, DurationInSeconds).From().SetEase(Ease);
		}
	}
}
