using System.Linq;
using UnityEngine;

namespace Code.UI.Tutorial {

	public class HighlightableElement : MonoBehaviour {
		public bool ProcessOnBlackoutClick = false;
		[SerializeField] private string HighlightType;

		public static GameObject FindGameObjectWithType(string type) {
			return FindObjectsByType<HighlightableElement>(FindObjectsSortMode.InstanceID)
				.FirstOrDefault(h => h.HighlightType == type)?.gameObject;
		}

		public void SetHighlightType(string type) {
			HighlightType = type;
		}
	}
}
