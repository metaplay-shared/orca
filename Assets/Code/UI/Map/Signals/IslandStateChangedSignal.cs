using Game.Logic;

namespace Code.UI.Map.Signals {
	public class IslandStateChangedSignal {
		public IslandTypeId IslandTypeId { get; }

		public IslandStateChangedSignal(IslandTypeId islandTypeId) {
			IslandTypeId = islandTypeId;
		}
	}
}
