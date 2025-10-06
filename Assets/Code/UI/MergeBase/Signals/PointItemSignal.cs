namespace Code.UI.MergeBase.Signals {
	public class PointItemSignal {
		public int X { get; private set; }
		public int Y { get; private set; }

		public PointItemSignal(int x, int y) {
			X = x;
			Y = y;
		}
	}
}
