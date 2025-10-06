using System;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerForceMismatch)]
	public class PlayerForceChecksumMismatch : PlayerAction {
		public IslandTypeId IslandId { get; private set; }

		public PlayerForceChecksumMismatch() { }

		public PlayerForceChecksumMismatch(IslandTypeId islandId) {
			IslandId = islandId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (commit) {
				player.EarnResources(CurrencyTypeId.Energy, new Random().Next(100), IslandId, ResourceModificationContext.Empty);
				if (new Random().Next(100) < 50) {
					player.Merge.EnergyFill.UpdateCurrentIndex(player.GameConfig);
				}
			}

			return ActionResult.Success;
		}
	}
}
