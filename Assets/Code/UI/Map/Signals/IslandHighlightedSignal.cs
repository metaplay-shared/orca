using Game.Logic;

namespace Code.UI.Map.Signals {
	public class IslandHighlightedSignal {
		public IslandTypeId Island { get; private set; }

		public IslandHighlightedSignal(IslandTypeId island) {
			Island = island;
		}
	}
}
