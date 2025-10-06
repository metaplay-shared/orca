using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerClaimLogbookTaskReward)]
	public class PlayerClaimLogbookTaskReward : PlayerAction {
		public LogbookTaskId TaskId { get; private set; }

		public PlayerClaimLogbookTaskReward() { }

		public PlayerClaimLogbookTaskReward(LogbookTaskId taskId) {
			TaskId = taskId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.GameConfig.LogbookTasks.ContainsKey(TaskId)) {
				return ActionResult.InvalidParam; // No such task
			}

			LogbookTaskInfo taskInfo = player.GameConfig.LogbookTasks[TaskId];
			if (!player.Logbook.Chapters.ContainsKey(taskInfo.Chapter)) {
				return ActionResult.InvalidParam; // No such chapter
			}

			if (player.Logbook.Chapters[taskInfo.Chapter].State != ChapterState.Open) {
				return ActionResult.InvalidState; // Chapter not open (yet)
			}

			if (!player.Logbook.Chapters[taskInfo.Chapter].Tasks.ContainsKey(TaskId)) {
				return ActionResult.InvalidParam; // No such task in chapter
			}

			LogbookTaskModel task = player.Logbook.Chapters[taskInfo.Chapter].Tasks[TaskId];
			if (!task.IsComplete || task.IsClaimed) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				player.Logbook.ClaimTaskReward(
					player.GameConfig,
					TaskId,
					player.CurrentTime,
					player,
					player.ClientListener
				);
				player.EarnResources(
					taskInfo.Reward.Type,
					taskInfo.Reward.Amount,
					IslandTypeId.None,
					new ResourceModificationContext()
				);
				player.EventStream.Event(new PlayerLogbookTaskRewardClaimed(TaskId));
				if (taskInfo.Reward.Type.WalletResource) {
					player.EventStream.Event(
						new PlayerEconomyAction(
							player,
							EconomyActionId.LogbookRewardClaimed,
							CurrencyTypeId.None,
							0,
							"",
							0,
							taskInfo.Reward.Type,
							taskInfo.Reward.Amount,
							""
						)
					);
				}
			}

			return ActionResult.Success;
		}
	}
}
