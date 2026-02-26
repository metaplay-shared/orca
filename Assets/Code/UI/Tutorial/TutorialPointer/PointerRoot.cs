using System;
using DG.Tweening;
using UnityEngine;

namespace Code.UI.Tutorial.TutorialPointer {
	public class PointerRoot : MonoBehaviour {
		[SerializeField] private PointerAnimation PointerAnimation;

		private Sequence sequence;

		public void Point(Vector3 pointPosition) {
			PointerAnimation.gameObject.SetActive(true);
			transform.position = pointPosition;
			PointerAnimation.Loop();
		}

		public void Swipe(Vector3 from, Vector3 to) {
			PointerAnimation.gameObject.SetActive(true);
			transform.position = from;

			PointerAnimation.ResetAndStop();
			
			sequence = DOTween.Sequence();

			sequence.Append(PointerAnimation.TweenPlayRange(0, 4));
			sequence.Append(transform.DOMove(to, 0.7f));
			sequence.Append(PointerAnimation.TweenPlayRange(5, 9));
			sequence.AppendInterval(1.0f);
			sequence.Append(transform.DOMove(from, 0.05f));
			sequence.AppendInterval(0.5f);

			sequence.SetLoops(-1);
		}

		public void Hide() {
			PointerAnimation.gameObject.SetActive(false);
			sequence.Kill();
			sequence = null;
		}

		private void Awake() {
			Hide();
		}
	}
}
