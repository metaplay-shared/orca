namespace Code.UI.MergeBase.Signals {
	public class MergeTaskSelectedSignal {
		public int SiblingIndex { get; }

		public MergeTaskSelectedSignal(int siblingIndex) {
			SiblingIndex = siblingIndex;
		}
	}
}
