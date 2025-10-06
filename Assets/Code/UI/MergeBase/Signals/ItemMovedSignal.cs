using Game.Logic;

namespace Code.UI.MergeBase.Signals {
	public class ItemMovedSignal {
		public string BoardIdentifier { get; }
		public ItemModel Item { get; }
		public int FromX { get; }
		public int FromY { get; }
		public int ToX { get; }
		public int ToY { get; }

		public ItemMovedSignal(string boardIdentifier, ItemModel item, int fromX, int fromY, int toX, int toY) {
			BoardIdentifier = boardIdentifier;
			Item = item;
			FromX = fromX;
			FromY = fromY;
			ToX = toX;
			ToY = toY;
		}
	}
}
