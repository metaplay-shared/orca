using DG.Tweening;
using System;
using UnityEngine;

namespace Code.UI.UITween {
	public abstract class EndlessUITween : MonoBehaviour {
		[SerializeField] private LoopType LoopType = LoopType.Yoyo;
		[SerializeField] private float SkipFirstSeconds = 0;

		private Tween tween;

		protected void Awake() {
			tween = CreateTween();
			tween.SetLoops(-1, LoopType).Goto(SkipFirstSeconds, andPlay: true);
		}

		protected void OnDestroy() {
			tween.Kill();
		}

		protected abstract Tween CreateTween();
	}
}
