using Game.Logic;

namespace Code.UI.Market {
	public class OpenMarketCategorySignal {
		public ShopCategoryId Category { get; private set; }

		public OpenMarketCategorySignal(ShopCategoryId category) {
			Category = category;
		}
	}
}
