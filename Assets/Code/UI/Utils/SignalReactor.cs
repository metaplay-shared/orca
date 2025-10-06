using UnityEngine;
using Zenject;

namespace Code.UI.Utils {
	public abstract class SignalReactor<TSignal> : MonoBehaviour {
		[Inject] private SignalBus signalBus;

		protected virtual void OnEnable() {
			signalBus.Subscribe<TSignal>(OnSignal);
		}

		protected virtual void OnDisable() {
			signalBus.Unsubscribe<TSignal>(OnSignal);
		}

		protected abstract void OnSignal(TSignal signal);
	}
}
