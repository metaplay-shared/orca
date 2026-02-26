using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Activables;
using Metaplay.Core.Model;

namespace Game.Logic {
	/// <summary>
	/// <para>
	/// <c>PlayerClaimActivityEventRewards</c> claims rewards earned during an activity event. The rewards can be
	/// claimed either manually (explicitly by the player) using this action or automatically.
	/// </para>
	/// <para>
	/// Rewards can be claimed manually at any time when the event is active. After the event has ended but before
	/// the start of the next occurrence the UI should force the player to claim the rewards. If the next occurrence of
	/// the event starts when there are unclaimed rewards, they are claimed automatically.
	/// </para>
	/// <para>
	/// Automatic claiming takes place also if the player starts a session when they have unclaimed rewards from
	/// an old occurrence of the event that is already past its review period. Technically the automatic claiming
	/// is triggered either by <see cref="ActivityEventModel.OnStartedActivation"/> hook or by
	/// <see cref="PlayerModel.GameOnSessionStarted()"/> (if the next occurrence of the event hasn't started yet).
	/// </para>
	/// </summary>
	[ModelAction(ActionCodes.PlayerClaimActivityEventReward)]
	public class PlayerClaimActivityEventRewards : PlayerAction {
		public EventId EventId { get; private set; }
		public PlayerClaimActivityEventRewards() { }

		public PlayerClaimActivityEventRewards(EventId eventId) {
			EventId = eventId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			ActivityEventModel activityEvent = player.ActivityEvents.TryGetState(EventId);
			if (activityEvent == null) {
				return ActionResult.NoSuchEvent;
			}

			if (activityEvent.Terminated) {
				return ActionResult.InvalidState;
			}

			List<ItemCountInfo> unclaimedRewards = activityEvent.UnclaimedRewards(player.GameConfig);
			if (unclaimedRewards.Count == 0) {
				return ActionResult.Success;
			}

			if (commit) {
				MetaTime now = player.CurrentTime;

				foreach (ItemCountInfo item in unclaimedRewards) {
					for (int i = 0; i < item.Count; i++) {
						// ChainTypeId not mapped to real type since it should always be a concrete type.
						// This is validated in SharedGameConfig.BuildTimeValidate().
						ItemModel model = new ItemModel(
							item.Type,
							item.Level,
							player.GameConfig,
							now,
							true
						);
						player.AddItemToHolder(
							model.Info.TargetIsland == IslandTypeId.All
								? IslandTypeId.MainIsland
								: model.Info.TargetIsland,
							model
						);
						player.HandleItemDiscovery(model);
					}
				}

				int rewardsClaimedBefore = activityEvent.ClaimedRewards();
				int claimedRewards = activityEvent.MarkRewardsClaimed(now, player.GameConfig, player.ClientListener);

				MetaActivableVisibleStatus eventStatus = player.Status(activityEvent);
				bool eventActive = eventStatus is MetaActivableVisibleStatus.Active ||
					eventStatus is MetaActivableVisibleStatus.EndingSoon;
				bool allRewardsClaimed = activityEvent.HasPremiumPass() &&
					!activityEvent.EventLevel.HasNextLevel(player.GameConfig);

				if (!eventActive || allRewardsClaimed) {
					activityEvent.Terminate();
				}

				player.ClientListener.OnEventStateChanged(EventId);
				player.EventStream.Event(
					new PlayerEventRewardsClaimed(
						EventId.Value,
						activityEvent.Info.ActivityEventType.Value,
						activityEvent.HasPremiumPass(),
						rewardsClaimedBefore,
						claimedRewards,
						autoClaim: false,
						activityEvent.StartTime
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
