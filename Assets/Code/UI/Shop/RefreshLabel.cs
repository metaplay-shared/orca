using Code.UI.Utils;
using Metaplay.Unity.DefaultIntegration;

namespace Code.UI.Shop {
	public class RefreshLabel : UpdateLabel {
		protected override string SourceText {
			get {
				var timeToRefresh = MetaplayClient.PlayerModel.Market.NextRefreshAt - MetaplayClient.PlayerModel.CurrentTime;
				return Localizer.Localize("View.Shop.RefreshLabel", timeToRefresh.ToSimplifiedString());
			}
		}

		protected override float UpdateIntervalSeconds => 1.0f;
	}
}
