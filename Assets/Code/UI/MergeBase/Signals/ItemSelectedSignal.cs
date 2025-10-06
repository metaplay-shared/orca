namespace Code.UI.MergeBase.Signals {
	public class ItemSelectedSignal {
		public IMergeItemModelAdapter Item { get; }

		public ItemSelectedSignal(IMergeItemModelAdapter item) {
			Item = item;
		}
	}
}
