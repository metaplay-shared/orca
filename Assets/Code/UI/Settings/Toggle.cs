using System;
using Code.UI.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Settings {
	public class Toggle : ButtonHelper {
		[SerializeField] private TMP_Text LabelText;
		[SerializeField] private RectTransform Knob;

		[SerializeField] private Sprite Off;
		[SerializeField] private Sprite On;

		private bool value = false;

		private Action<bool> callback;

		public void Setup(string label, bool initialValue, Action<bool> callback) {
			value = initialValue;
			this.callback = callback;
			LabelText.text = label;

			Animate();
		}

		protected override void OnClick() {
			value = !value;
			callback.Invoke(value);

			Animate();
		}

		private void Animate() {
			GetComponent<Image>().sprite = value ? On : Off;
			float x = value ? 1 : 0;

			Knob.anchorMin = new Vector2(x, Knob.anchorMin.y);
			Knob.anchorMax = new Vector2(x, Knob.anchorMax.y);
			Knob.anchoredPosition = Vector2.zero;
		}
	}
}
