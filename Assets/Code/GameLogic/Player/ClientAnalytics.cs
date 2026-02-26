using Metaplay.Core.Analytics;
using Metaplay.Core.Model;

namespace Game.Logic {
	public static class ClientEventCodes {
		public const int ShopOpened = 3001;
		public const int AppOpened = 3002;
	}

	[AnalyticsEvent(ClientEventCodes.ShopOpened)]
	public class ClientShopOpened : ClientEventBase {
		public ClientShopOpened() { }
	}

	[AnalyticsEvent(ClientEventCodes.AppOpened)]
	public class ClientAppOpened : ClientEventBase {
		public ClientAppOpened() { }
	}
}
