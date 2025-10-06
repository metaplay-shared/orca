using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerFillEnergy)]
	public class PlayerFillEnergy : PlayerAction {
		public IslandTypeId IslandId { get; private set; }

		public PlayerFillEnergy() { }

		public PlayerFillEnergy(IslandTypeId islandId) {
			IslandId = islandId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			EnergyCostInfo costInfo = player.Merge.EnergyFill.EnergyCost(player.GameConfig);
			if (player.Wallet.Currency(costInfo.CurrencyType).Value < costInfo.Cost) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				player.EarnResources(CurrencyTypeId.Energy, player.MaxEnergy, IslandId, ResourceModificationContext.Empty);
				player.ConsumeResources(costInfo.CurrencyType, costInfo.Cost, ResourceModificationContext.Empty);
				player.Merge.EnergyFill.UpdateCurrentIndex(player.GameConfig);
				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.FillEnergy,
						costInfo.CurrencyType,
						costInfo.Cost,
						"",
						0,
						CurrencyTypeId.Energy,
						player.MaxEnergy,
						""
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
