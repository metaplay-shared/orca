using Game.Logic;

namespace Code.UI.Map.Signals {
	public class MapControlsZoomedCloseToIslandSignal {
		public readonly IslandInfo Island;

		public MapControlsZoomedCloseToIslandSignal(IslandInfo island) {
			Island = island;
		}
	}
}
