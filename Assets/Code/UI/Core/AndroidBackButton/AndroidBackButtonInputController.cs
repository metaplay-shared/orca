using System;
using Orca.Unity.PlayerLoop;
using UnityEngine;
using Zenject;

namespace Code.UI.Core.AndroidBackButton {
	public interface IAndroidBackButtonInputController {
		event Action OnBackButtonPressed;
	}

	public class AndroidBackButtonInputController :
		IAndroidBackButtonInputController,
		IInitializable,
		IDisposable {
		private readonly IUpdateUnityEventMediator updateUnityEventMediator;

		public AndroidBackButtonInputController(IUpdateUnityEventMediator updateUnityEventMediator) {
			this.updateUnityEventMediator = updateUnityEventMediator;
		}

		public event Action OnBackButtonPressed;

		void IInitializable.Initialize() {
			updateUnityEventMediator.OnUpdate += OnUpdate;
		}

		void IDisposable.Dispose() {
			updateUnityEventMediator.OnUpdate -= OnUpdate;
		}

		private void OnUpdate() {
			if (Input.GetKeyDown(KeyCode.Escape)) {
				OnBackButtonPressed?.Invoke();
			}
		}
	}
}
