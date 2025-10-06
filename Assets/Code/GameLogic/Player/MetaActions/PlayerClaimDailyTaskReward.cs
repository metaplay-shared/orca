using Metaplay.Core.Model;

namespace Game.Logic {

	[ModelAction(ActionCodes.PlayerClaimDailyTaskReward)]
	public class PlayerClaimDailyTaskReward : PlayerAction {
		public EventId EventId { get; private set; }
		public int TaskSlot { get; private set; }
		public PlayerClaimDailyTaskReward() { }

		public PlayerClaimDailyTaskReward(EventId eventId, int taskSlot) {
			EventId = eventId;
			TaskSlot = taskSlot;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			DailyTaskEventModel dailyTaskEvent = player.DailyTaskEvents.TryGetState(EventId);
			if (dailyTaskEvent == null) {
				return ActionResult.NoSuchEvent;
			}

			if (dailyTaskEvent.Terminated) {
				return ActionResult.InvalidState;
			}

			DailyTaskItem taskItem = dailyTaskEvent.Tasks[TaskSlot];
			if (!taskItem.Completed || taskItem.RewardClaimed) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				int rewardsClaimedBefore = dailyTaskEvent.ClaimedRewards();
				dailyTaskEvent.ClaimReward(player, TaskSlot);
				player.ClientListener.OnEventStateChanged(dailyTaskEvent.ActivableId);
				player.EventStream.Event(
					new PlayerEventRewardsClaimed(
						EventId.Value,
						dailyTaskEvent.Info.DailyTaskSetId.Value,
						hasPremiumPass: false,
						rewardsClaimedBefore,
						1,
						autoClaim: false,
						dailyTaskEvent.StartTime
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
