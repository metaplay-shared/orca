using Game.Logic;

namespace Code.UI.Application.Signals {
	public class ApplicationPauseSignal {
		public bool Paused { get; }

		public ApplicationPauseSignal(bool paused) {
			Paused = paused;
		}
	}

	public class ApplicationFocusSignal {
		public bool Focused { get; }

		public ApplicationFocusSignal(bool focused) {
			Focused = focused;
		}
	}
}
