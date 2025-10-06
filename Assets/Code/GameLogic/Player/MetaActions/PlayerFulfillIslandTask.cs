using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerFulfillIslandTask)]
	public class PlayerFulfillIslandTask : PlayerAction {
		public IslandTypeId Island { get; private set; }
		public IslanderId Islander { get; private set; }

		public PlayerFulfillIslandTask() { }

		public PlayerFulfillIslandTask(IslandTypeId island, IslanderId islander) {
			Island = island;
			Islander = islander;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(Island)) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[Island];
			if (island.State != IslandState.Open) {
				return ActionResult.InvalidState;
			}
			MergeBoardModel mergeBoard = island.MergeBoard;

			if (!island.Tasks.Tasks.ContainsKey(Islander)) {
				return ActionResult.InvalidParam;
			}

			IslandTaskModel task = island.Tasks.Tasks[Islander];
			if (!task.Enabled) {
				return ActionResult.InvalidState;
			}

			if (!mergeBoard.HasItemsForTask(task.Info)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				int taskId = island.Tasks.CompleteTask(Islander);
				island.Tasks.UpdateTasks(island.Info, player.GameConfig, player.ClientListener);

				// Note, items must be removed after the task has been completed. Otherwise the UI side will reflect
				// wrong task state on items that are required in tasks.
				foreach (ItemCountInfo items in task.Info.Items) {
					mergeBoard.RemoveItems(items.Type, items.Level, items.Count, false, player.ClientListener);
				}

				if (task.Info.RewardResources.Count > 0 || task.Info.RewardItems.Count > 0) {
					RewardModel reward = new RewardModel(
						task.Info.RewardResources,
						task.Info.RewardItems,
						ChainTypeId.IslandRewards,
						1,
						new RewardMetadata() {
							Island = Island,
							Type = RewardType.IslandTask
						}
					);
					player.AddReward(reward);
				}

				player.EarnResources(CurrencyTypeId.Xp, task.Info.PlayerXp, Island, new IslanderTaskResourceContext(Islander));
				island.AddIslandXp(player.GameConfig, task.Info.IslandXp, player.AddReward, player.ClientListener, player.ServerListener);

				player.EventStream.Event(new PlayerIslandTaskFulfilled(Island, Islander, taskId));

				foreach (TriggerId trigger in task.Info.Triggers) {
					player.Triggers.ExecuteTrigger(player, trigger);
				}
			}

			return ActionResult.Success;
		}
	}
}
