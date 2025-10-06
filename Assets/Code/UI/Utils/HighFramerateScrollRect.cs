using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Utils {
	[RequireComponent(typeof(ScrollRect))]
	public class HighFramerateScrollRect : HighFramerateBehaviourBase {
		private ScrollRect scrollRect;

		protected void Awake() {
			scrollRect = GetComponent<ScrollRect>();
		}

		protected override bool IsMoving() => scrollRect.velocity.sqrMagnitude > 0;
	}
}
