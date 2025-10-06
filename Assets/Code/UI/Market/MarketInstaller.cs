using Zenject;

namespace Code.UI.Market {
	public class MarketInstaller : Installer {
		public override void InstallBindings() {
			Container.DeclareSignal<MarketItemUpdatedSignal>().OptionalSubscriber();
			Container.DeclareSignal<OpenMarketCategorySignal>().OptionalSubscriber();
			Container.DeclareSignal<MarketUpdatedSignal>().OptionalSubscriber();
		}
	}
}
