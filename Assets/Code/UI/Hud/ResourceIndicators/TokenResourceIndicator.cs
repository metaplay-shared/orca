using Code.UI.HudBase;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;

namespace Code.UI.Hud.ResourceIndicators {
	public class TokenResourceIndicator : ResourceIndicatorBase {
		protected override int ResourceAmount => MetaplayClient.PlayerModel.Wallet.IslandTokens.Value;
		protected override CurrencyTypeId Type => CurrencyTypeId.IslandTokens;

		public override void OnClick() {
			if (MetaplayClient.PlayerModel.PrivateProfile.FeaturesEnabled.Contains(
					FeatureTypeId.HudButtonIslandTokens
				)) {
				Debug.LogWarning("Token indicator clicked");
			}
		}
	}
}
