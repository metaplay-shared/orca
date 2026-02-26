using Game.Logic;

namespace Code.UI.Tutorial {
	public class MergeHintSignal {
		public ItemModel ItemModelA { get; }
		public ItemModel ItemModelB { get; }

		public MergeHintSignal(ItemModel itemModelA, ItemModel itemModelB) {
			ItemModelA = itemModelA;
			ItemModelB = itemModelB;
		}
	}
}
