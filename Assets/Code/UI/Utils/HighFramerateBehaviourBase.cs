using System;
using Code.UI.Application;
using Orca.Common;
using UnityEngine;
using Zenject;

namespace Code.UI.Utils {
	public abstract class HighFramerateBehaviourBase : MonoBehaviour {
		[Inject] private void Set(IFrameRateController controller) => frameRateController = controller.ToOption();

		private Option<IDisposable> highFrameRateHandle;
		private Option<IFrameRateController> frameRateController;

		protected void OnEnable() {
			if (!frameRateController.HasValue) {
				Debug.LogWarning("No frame rate controller injected.");
			}
		}

		protected void Update() {
			if (!frameRateController.HasValue) {
				// Early out to save performance
			}

			if (highFrameRateHandle.HasValue) {
				if (!IsMoving()) {
					DisposeHighFrameRateHandle();
				}
			} else {
				if (IsMoving()) {
					highFrameRateHandle = frameRateController.Map(c => c.RequestHighFPS());
				}
			}
		}

		protected abstract bool IsMoving();

		protected void OnDestroy() {
			DisposeHighFrameRateHandle();
		}

		private void DisposeHighFrameRateHandle() {
			foreach (IDisposable frameRateHandle in highFrameRateHandle) {
				frameRateHandle.Dispose();
				highFrameRateHandle = default;
			}
		}
	}
}
