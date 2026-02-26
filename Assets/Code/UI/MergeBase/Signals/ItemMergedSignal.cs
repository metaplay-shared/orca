using Game.Logic;

namespace Code.UI.MergeBase.Signals {
	public class ItemMergedSignal {
		public IslandTypeId Island { get; }
		public ItemModel Item { get; }

		public ItemMergedSignal(IslandTypeId island, ItemModel item) {
			Island = island;
			Item = item;
		}
	}
}
