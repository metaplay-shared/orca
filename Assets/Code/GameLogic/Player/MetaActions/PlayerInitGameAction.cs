using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerInitGame)]
	public class PlayerInitGameAction : PlayerAction {
		public PlayerInitGameAction() { }

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (commit) {
				player.InitGame();
			}

			return ActionResult.Success;
		}
	}
}
