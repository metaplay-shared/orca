using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerUpgradeBackpack)]
	public class PlayerUpgradeBackpack : PlayerAction {
		public PlayerUpgradeBackpack() { }

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.GameConfig.BackpackLevels.ContainsKey(player.Backpack.Info.Level + 1)) {
				return ActionResult.InvalidState;
			}

			BackpackLevelInfo upgraded = player.GameConfig.BackpackLevels[player.Backpack.Info.Level + 1];
			if (!player.Wallet.EnoughCurrency(upgraded.UnlockCost.Type, upgraded.UnlockCost.Amount)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				player.ConsumeResources(upgraded.UnlockCost.Type, upgraded.UnlockCost.Amount, ResourceModificationContext.Empty);
				player.Backpack.Upgrade(player.GameConfig);
				player.ClientListener.OnBackpackUpgraded();
				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.BackpackUpgraded,
						upgraded.UnlockCost.Type,
						upgraded.UnlockCost.Amount,
						"",
						0,
						CurrencyTypeId.None,
						0,
						""
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
