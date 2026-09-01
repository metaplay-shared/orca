using Game.Logic;

namespace Code.UI.Merge.AddOns.MergeBoard.LockArea {
	public class AreaStateChangedSignal {
		public IslandTypeId IslandId { get; }
		public string Index { get; }
		public AreaState State { get; }

		public AreaStateChangedSignal(IslandTypeId islandId, string index, AreaState state) {
			IslandId = islandId;
			Index = index;
			State = state;
		}
	}
}
