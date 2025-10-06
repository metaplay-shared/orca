using Code.UI.Application;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Utils {
	[RequireComponent(typeof(Button))]
	public abstract class ButtonHelper : MonoBehaviour {
		[Inject] protected SignalBus signalBus;

		protected Button Button;

		protected virtual void OnEnable() {
			Button = GetComponent<Button>();
			Button.onClick.AddListener(OnClickInternal);
		}

		protected virtual void OnDisable() {
			Button.onClick.RemoveListener(OnClickInternal);
		}

		private void OnClickInternal() {
			signalBus.Fire(new ButtonClickedSignal());
			OnClick();
		}

		protected abstract void OnClick();
	}

	public class ButtonClickedSignal { }
}
