using Game.Logic;

namespace Code.UI.MergeBase.Signals {
	public class ItemStateChangedSignal {
		public IslandTypeId Island { get; }
		public ItemModel Item { get; }

		public ItemStateChangedSignal(IslandTypeId island, ItemModel item) {
			Island = island;
			Item = item;
		}
	}
}
