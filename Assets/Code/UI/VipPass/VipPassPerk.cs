using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.VipPass {
	public class VipPassPerk : MonoBehaviour {
		[SerializeField] private Image Icon;
		[SerializeField] private TMP_Text Label;

		public void Setup(Sprite icon, string label) {
			Icon.sprite = icon;
			Label.text = label;
		}
	}
}
