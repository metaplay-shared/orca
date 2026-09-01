using Game.Logic;

namespace Code.UI.Map.Signals {
	public class IslandRemovedSignal {
		public IslandTypeId Island { get; private set; }

		public IslandRemovedSignal(IslandTypeId island) {
			Island = island;
		}
	}
}
