using Game.Logic;

namespace Code.UI.Map.Signals {
	public class IslandPointedSignal {
		public IslandTypeId Island { get; private set; }

		public IslandPointedSignal(IslandTypeId island) {
			Island = island;
		}
	}
}
