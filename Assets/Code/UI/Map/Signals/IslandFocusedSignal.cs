using Game.Logic;

namespace Code.UI.Map.Signals {
	public class IslandFocusedSignal {
		public IslandTypeId Island { get; private set; }

		public IslandFocusedSignal(IslandTypeId island) {
			Island = island;
		}
	}
}
