using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Zenject;

namespace Code.UI.Core.AndroidBackButton {
	public interface IAndroidBackButtonHandler {
		void HandleAndroidBackButtonPressed();
	}

	public interface IAndroidBackButtonController {
		void AddBackButtonHandler(IAndroidBackButtonHandler handler);
		void RemoveBackButtonHandler(IAndroidBackButtonHandler handler);
		AndroidBackButtonLock LockBackButton();
		void UnlockBackButton(AndroidBackButtonLock backButtonLock);
	}

	public class AndroidBackButtonLock : IDisposable {
		private readonly IAndroidBackButtonController backButtonController;

		public AndroidBackButtonLock(IAndroidBackButtonController backButtonController) {
			this.backButtonController = backButtonController;
		}

		public void Dispose() {
			backButtonController.UnlockBackButton(this);
		}
	}

	[UsedImplicitly]
	public class AndroidBackButtonController :
		IAndroidBackButtonController,
		IInitializable,
		IDisposable {
		private readonly IAndroidBackButtonInputController androidBackButtonInputController;
		private readonly List<IAndroidBackButtonHandler> handlerStack = new List<IAndroidBackButtonHandler>();
		private readonly List<AndroidBackButtonLock> locks = new List<AndroidBackButtonLock>();

		public AndroidBackButtonController(IAndroidBackButtonInputController androidBackButtonInputController) {
			this.androidBackButtonInputController = androidBackButtonInputController;
		}
		
		void IInitializable.Initialize() {
			androidBackButtonInputController.OnBackButtonPressed += OnBackButtonPressed;
		}
		
		void IDisposable.Dispose() {
			androidBackButtonInputController.OnBackButtonPressed -= OnBackButtonPressed;
		}

		private void OnBackButtonPressed() {
			HandleAndroidBackButtonPressed();
		}

		public void AddBackButtonHandler(IAndroidBackButtonHandler handler) {
			handlerStack.Add(handler);
		}

		public void RemoveBackButtonHandler(IAndroidBackButtonHandler handler) {
			handlerStack.Remove(handler);
		}

		private void HandleAndroidBackButtonPressed() {
			if (locks.Count > 0) {
				return;
			}

			switch (handlerStack.LastOrDefault()) {
				case IAndroidBackButtonHandler handler: {
					handler.HandleAndroidBackButtonPressed();
					break;
				}
			}
		}

		public AndroidBackButtonLock LockBackButton() {
			var backButtonLock = new AndroidBackButtonLock(this);
			locks.Add(backButtonLock);
			return backButtonLock;
		}

		public void UnlockBackButton(AndroidBackButtonLock backButtonLock) {
			locks.Remove(backButtonLock);
		}
	}
}
