using Game.Logic;

namespace Code.UI.MergeBase.Signals {
	public class MergeBoardStateChangedSignal {
		public IslandTypeId Island { get; }

		public MergeBoardStateChangedSignal(IslandTypeId island) {
			Island = island;
		}
	}
}
