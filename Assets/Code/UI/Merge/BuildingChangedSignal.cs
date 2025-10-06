using Game.Logic;

namespace Code.UI.Merge {
	public class BuildingChangedSignal {
		public IslandTypeId Island { get; }

		public BuildingChangedSignal(IslandTypeId island) {
			Island = island;
		}
	}
}
