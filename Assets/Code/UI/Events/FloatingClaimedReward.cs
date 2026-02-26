using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Code.UI.Events {
	public class FloatingClaimedReward : MonoBehaviour {
		[SerializeField] private TMP_Text Label;
		[SerializeField] private CanvasGroup CanvasGroup;
		public void Show(string text) {
			Label.text = text;
			DOTween.Sequence()
				.Join(transform.DOLocalMoveY(100, 1.0f).SetRelative(true).SetEase(Ease.OutQuad))
				.Insert(0.5f, CanvasGroup.DOFade(0, 0.5f))
				.AppendCallback(() => Destroy(gameObject))
				;
		}
	}
}
