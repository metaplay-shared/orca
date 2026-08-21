using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace Code.UI.Tutorial {
	public class UiElementHighlight : Highlight {
		UniTaskCompletionSource tcs = new();

		protected override void PreProcess() {
			foreach (var highlightedObject in highlightedObjects) {
				foreach (var button in highlightedObject.GameObject.GetComponentsInChildren<Button>()) {
					button.onClick.AddListener(ButtonPressed);
				}
				if (highlightedObject.ProcessOnBlackoutClick) {
					blackout.OnClick.AddListener(ButtonPressed);
				}
			}
		}

		protected override async UniTask Wait() {
			await tcs.Task;
		}

		protected override void PostProcess() {
			foreach (var highlightedObject in highlightedObjects) {
				if (highlightedObject == null ||
					highlightedObject.GameObject == null ||
					!highlightedObject.GameObject.activeInHierarchy) {
					return;
				}

				foreach (var button in highlightedObject.GameObject.GetComponentsInChildren<Button>()) {
					button.onClick.RemoveListener(ButtonPressed);
				}

				if (highlightedObject.ProcessOnBlackoutClick) {
					blackout.OnClick.RemoveListener(ButtonPressed);
				}
			}
		}

		private void ButtonPressed() {
			tcs.TrySetResult();
		}
	}
}
