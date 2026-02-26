using Game.Logic;

namespace Code.UI.MergeBase.Signals {
	public class BuilderUsedSignal {
		public IslandTypeId Island { get; }
		public ItemModel Item { get; }
		public int Duration { get; }

		public BuilderUsedSignal(IslandTypeId island, ItemModel item, int duration) {
			Island = island;
			Item = item;
			Duration = duration;
		}
	}
}
