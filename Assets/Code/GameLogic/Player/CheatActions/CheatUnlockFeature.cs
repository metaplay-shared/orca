using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(CheatActionCodes.CheatUnlockFeature)]
	[DevelopmentOnlyAction]
	public class CheatUnlockFeature : PlayerAction {
		public FeatureTypeId Feature { get; private set; }

		public CheatUnlockFeature() { }

		public CheatUnlockFeature(FeatureTypeId feature) {
			Feature = feature;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (player.PrivateProfile.FeaturesEnabled.Contains(Feature)) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				player.UnlockFeature(Feature);
			}

			return MetaActionResult.Success;
		}
	}
}
