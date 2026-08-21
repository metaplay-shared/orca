using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerEnterIsland)]
	public class PlayerEnterIsland : PlayerAction {
		public IslandTypeId IslandId { get; private set; }

		public PlayerEnterIsland() { }

		public PlayerEnterIsland(IslandTypeId islandId) {
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

			if (commit) {
				island.UpdateBuildingState(
					player.GameConfig,
					player.HandleBuildingState,
					player.ClientListener
				);
				foreach (TriggerId trigger in island.Info.EnterTriggers) {
					player.Triggers.ExecuteTrigger(player, trigger);
				}
				player.EventStream.Event(new PlayerIslandEntered(IslandId));
				player.CurrentIsland = IslandId;
				player.LastIsland = IslandId;
			}

			return ActionResult.Success;
		}
	}
}
