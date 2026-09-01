using Code.UI.Utils;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;

namespace Code.UI.Hud {
	public class VipPassTimeLeftLabel : UpdateLabel {
		protected override string SourceText {
			get {
				MetaDuration duration =
					MetaplayClient.PlayerModel.VipPasses.PassDuration(MetaplayClient.PlayerModel.CurrentTime);
				if (duration >= MetaDuration.FromDays(2)) {
					return Localizer.Localize("VipPass.Label.DaysLeft", duration.Milliseconds / (24 * 3600_000));
				} else if (duration >= MetaDuration.FromDays(1)) {
					return Localizer.Localize("VipPass.Label.DayLeft");
				} else {
					return duration.ToSimplifiedString();
				}
			}
		}

		protected override float UpdateIntervalSeconds => 1;
	}
}
