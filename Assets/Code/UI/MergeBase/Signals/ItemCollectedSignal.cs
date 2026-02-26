namespace Code.UI.MergeBase.Signals {
	public class ItemCollectedSignal {
		public int X { get; }
		public int Y { get; }

		public ItemCollectedSignal(int x, int y) {
			X = x;
			Y = y;
		}
	}
}
