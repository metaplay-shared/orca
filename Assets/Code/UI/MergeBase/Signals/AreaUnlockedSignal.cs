namespace Code.UI.MergeBase.Signals {
	public class AreaUnlockedSignal {
		public int AreaIndex { get; }

		public AreaUnlockedSignal(int areaIndex) {
			AreaIndex = areaIndex;
		}
	}
}
