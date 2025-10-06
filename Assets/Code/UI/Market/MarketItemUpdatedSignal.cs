using Game.Logic;

namespace Code.UI.Market {
	public class MarketItemUpdatedSignal {
		public LevelId<ShopCategoryId> ItemId { get; private set; }

		public MarketItemUpdatedSignal(LevelId<ShopCategoryId> itemId) {
			ItemId = itemId;
		}
	}
}
