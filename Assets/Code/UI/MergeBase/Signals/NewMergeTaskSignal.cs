namespace Code.UI.MergeBase.Signals {
	public class NewMergeTaskSignal {
		public int SlotIndex { get; }

		public NewMergeTaskSignal(int slotIndex) {
			SlotIndex = slotIndex;
		}
	}
}
