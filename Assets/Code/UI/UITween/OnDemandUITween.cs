using DG.Tweening;
using UnityEngine;

namespace Code.UI.UITween {
	public abstract class OnDemandUITween : MonoBehaviour {
		protected abstract Tween CreateTween();
	}
}
