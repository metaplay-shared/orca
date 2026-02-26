namespace Code.UI.MergeBase.Signals {
	public class ItemRemovedSignal {
		public string IslandIdentifier { get; }
		public int X { get; }
		public int Y { get; }

		public ItemRemovedSignal(string islandIdentifier, int x, int y) {
			IslandIdentifier = islandIdentifier;
			X = x;
			Y = y;
		}
	}
}
