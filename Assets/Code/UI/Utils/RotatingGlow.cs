using UnityEngine;

namespace Code.UI.Utils {
	public class RotatingGlow : MonoBehaviour {
		[SerializeField] private Transform Icon;
		[SerializeField] private int Speed = 50;

		private void Update() {
			Icon.Rotate(0, 0, Time.deltaTime * Speed);
		}
	}
}
