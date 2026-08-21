using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerRevealIsland)]
	public class PlayerRevealIsland : PlayerAction {
		public IslandTypeId Island { get; private set; }

		public PlayerRevealIsland() { }

		public PlayerRevealIsland(IslandTypeId island) {
			Island = island;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(Island)) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[Island];
			if (island.State != IslandState.Revealing) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				island.ModifyState(
					island.Info.UnlockCost.Amount > 0 ? IslandState.Locked : IslandState.Open,
					player.GameConfig,
					player.CurrentTime,
					player.HandleItemDiscovery,
					player.ClientListener
				);
				player.UnlockIslands();
				if (island.Info.UnlockCost.Amount > 0) {
					foreach (TriggerId trigger in island.Info.RevealTriggers) {
						player.Triggers.ExecuteTrigger(player, trigger);
					}
				} else {
					foreach (TriggerId trigger in island.Info.UnlockTriggers) {
						player.Triggers.ExecuteTrigger(player, trigger);
					}
				}

				player.EventStream.Event(new PlayerIslandRevealed(Island));
			}

			return ActionResult.Success;
		}
	}
}
