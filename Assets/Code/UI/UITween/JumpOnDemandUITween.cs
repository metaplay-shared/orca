using DG.Tweening;
using Orca.Common;
using UnityEngine;

namespace Code.UI.UITween {
	public class JumpOnDemandUITween : OnDemandUITween {
		[SerializeField] private RectTransform RectTransform;
		[SerializeField] private float Duration = 1f;
		[SerializeField] private float JumpHeight = 10f;
		[SerializeField] private Vector2 JumpPunchScale = Vector2.zero;

		private Option<Tween> tween;

		private void Reset() {
			RectTransform = GetComponent<RectTransform>();
		}

		private void Awake() {
			tween = CreateTween().SetAutoKill(false).SetLink(gameObject).Pause();
		}

		protected override Tween CreateTween() {
			var sequence = DOTween.Sequence();
			sequence.Join(RectTransform.DOJumpAnchorPos(Vector2.zero, JumpHeight, 1, Duration).SetRelative());
			sequence.Join(RectTransform.DOPunchScale(JumpPunchScale, Duration, 0, 0f).SetRelative());
			return sequence;
		}

		public void Play(float duration) {
			Debug.Assert(tween.HasValue, "Tween has not been created yet");
			foreach (var t in tween) {
				t.timeScale = Duration / duration;
				t.Rewind();
				t.Play();
			}
		}
	}
}
