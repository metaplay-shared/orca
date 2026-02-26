using Code.UI.Application;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.Utils {
	public abstract class UpdateLabel : StaticLabel {
		protected abstract float UpdateIntervalSeconds { get; }

		private float lastUpdate;
		private void Update() {
			if (lastUpdate + UpdateIntervalSeconds < Time.time) {
				UpdateText();
				lastUpdate = Time.time;
			}
		}
	}

	public abstract class SignalLabel<TSignal> : StaticLabel {
		[Inject] private SignalBus signalBus;

		protected override void OnEnable() {
			signalBus.Subscribe<TSignal>(OnSignalFired);
			base.OnEnable();
		}

		private void OnDisable() {
			signalBus.Unsubscribe<TSignal>(OnSignalFired);
		}

		private void OnSignalFired() {
			UpdateText();
		}
	}

	public abstract class StaticLabel : MonoBehaviour {
		protected abstract string SourceText { get; }
		protected TMP_Text text;

		protected virtual void OnEnable() {
			text = GetComponent<TMP_Text>();
			UpdateText();
		}

		protected void UpdateText() {
			text.text = SourceText;
		}
	}
}
