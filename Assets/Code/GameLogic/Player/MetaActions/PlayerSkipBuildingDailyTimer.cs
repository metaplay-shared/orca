using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSkipBuildingDailyTimer)]
	public class PlayerSkipBuildingDailyTimer : PlayerAction {
		public IslandTypeId IslandId { get; private set; }

		public PlayerSkipBuildingDailyTimer() { }

		public PlayerSkipBuildingDailyTimer(IslandTypeId islandId) {
			IslandId = islandId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(IslandId)) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[IslandId];
			if (island.State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			if (island.BuildingState != BuildingState.Complete) {
				return ActionResult.InvalidState;
			}

			int timeLeft = F64.CeilToInt(
				island.TimeToDailyRewards(player.GameConfig, player.CurrentTime).ToSecondsF64()
			);

			Cost cost = island.SkipCreatorTimerCost(player.GameConfig, player.CurrentTime);
			if (cost.Amount <= 0) {
				return ActionResult.Success;
			}

			if (!player.Wallet.EnoughCurrency(cost.Type, cost.Amount)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				player.ConsumeResources(cost.Type, cost.Amount, ResourceModificationContext.Empty);
				island.MarkBuildingDailyRewardClaimed(MetaTime.Epoch);

				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.BuildingDailyTimerSkipped,
						cost.Type,
						cost.Amount,
						"",
						0,
						CurrencyTypeId.Time,
						timeLeft,
						""
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
