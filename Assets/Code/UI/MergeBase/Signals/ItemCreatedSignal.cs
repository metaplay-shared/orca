using Game.Logic;

namespace Code.UI.MergeBase.Signals {
	public class ItemCreatedSignal {
		public IslandTypeId IslandTypeId { get; }
		public IMergeItemModelAdapter Item { get; }
		public int FromX { get; }
		public int FromY { get; }
		public bool FromItemHolder { get; }
		public bool Spawned { get; }

		public ItemCreatedSignal(
			IslandTypeId islandTypeId,
			IMergeItemModelAdapter item,
			int fromX,
			int fromY,
			bool fromItemHolder,
			bool spawned
		) {
			IslandTypeId = islandTypeId;
			Item = item;
			FromX = fromX;
			FromY = fromY;
			FromItemHolder = fromItemHolder;
			Spawned = spawned;
		}
	}
}
