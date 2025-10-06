using TMPro;
using UnityEngine;

namespace Code.UI.Utils {
	[RequireComponent(typeof(TMP_Text))]
	public class LocalizedLabel : MonoBehaviour {
		[SerializeField] private string LocalizationKey;

		private TMP_Text label;

		private void Awake() {
			label = GetComponent<TMP_Text>();
		}

		private void Start() {
			label.text = Localizer.Localize(LocalizationKey);
		}
	}
}
