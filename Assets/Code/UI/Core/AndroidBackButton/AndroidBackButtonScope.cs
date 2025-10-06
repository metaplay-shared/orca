using System;

namespace Code.UI.Core.AndroidBackButton {
	public readonly struct AndroidBackButtonScope : IDisposable {
		private readonly IAndroidBackButtonController controller;
		private readonly IAndroidBackButtonHandler handler;

		public AndroidBackButtonScope(IAndroidBackButtonController controller, IAndroidBackButtonHandler handler) {
			this.controller = controller;
			this.handler = handler;
			
			controller.AddBackButtonHandler(handler);
		}

		public void Dispose() {
			controller.RemoveBackButtonHandler(handler);
		}
	}
}
