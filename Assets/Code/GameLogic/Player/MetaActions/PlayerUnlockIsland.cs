using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerUnlockIsland)]
	public class PlayerUnlockIsland : PlayerAction {
		public IslandTypeId Island { get; private set; }

		public PlayerUnlockIsland() { }

		public PlayerUnlockIsland(IslandTypeId island) {
			Island = island;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(Island)) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[Island];
			if (island.State != IslandState.Locked) {
				return ActionResult.InvalidState;
			}

			if (!player.Wallet.EnoughCurrency(island.Info.UnlockCost.Type, island.Info.UnlockCost.Amount)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				player.ConsumeResources(island.Info.UnlockCost.Type, island.Info.UnlockCost.Amount, ResourceModificationContext.Empty);
				island.ModifyState(
					IslandState.Open,
					player.GameConfig,
					player.CurrentTime,
					player.HandleItemDiscovery,
					player.ClientListener
				);
				player.Logbook.RegisterTaskProgress(LogbookTaskType.UnlockIsland, Island, player.CurrentTime, player.ClientListener);
				player.UnlockIslands();
				foreach (TriggerId trigger in island.Info.UnlockTriggers) {
					player.Triggers.ExecuteTrigger(player, trigger);
				}

				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.IslandUnlocked,
						island.Info.UnlockCost.Type,
						island.Info.UnlockCost.Amount,
						"",
						0,
						CurrencyTypeId.None,
						0,
						Island.Value
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
