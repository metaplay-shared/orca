using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	public abstract class DelayedUITween : MonoBehaviour {
		[SerializeField] private float DelayInSeconds = 0;

		private Tween tween;

		protected void Awake() {
			tween = CreateTween()
				.SetDelay(DelayInSeconds)
				.SetAutoKill(false)
				.Pause();
		}

		protected void OnEnable() {
			tween.Play();
		}

		protected void OnDisable() {
			tween.Rewind();
		}

		protected void OnDestroy() {
			tween.Kill();
		}

		protected abstract Tween CreateTween();
	}
}
