using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Metaplay.Core;
using Metaplay.Core.Activables;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using Metaplay.Core.Schedule;

namespace Game.Logic {
	[MetaSerializableDerived(3)]
	public class DailyTaskEventsModel : MetaActivableSet<EventId, DailyTaskEventInfo, DailyTaskEventModel> {
		protected override DailyTaskEventModel CreateActivableState(DailyTaskEventInfo info, IPlayerModelBase player) {
			return new DailyTaskEventModel(info);
		}

		public DailyTaskEventModel SubEnsureHasState(DailyTaskEventInfo info, IPlayerModelBase player) {
			return EnsureHasState(info, player);
		}
	}

	[MetaSerializableDerived(3)]
	public class DailyTaskEventModel : MetaActivableState<EventId, DailyTaskEventInfo>, IEventModel {
		[MetaMember(1)] public sealed override EventId ActivableId { get; protected set; }
		[MetaMember(2)] public List<DailyTaskItem> Tasks { get; protected set; }

		[MetaMember(3)] public MetaTime AdSeenTime { get; protected set; }
		[MetaMember(4)] public bool Terminated { get; protected set; }
		[MetaMember(5)] public MetaTime StartTime { get; protected set; }

		[MetaMember(6)] public int Level { get; protected set; }
		[MetaMember(7)] public int AppliedLevelPenalties { get; protected set; }

		[IgnoreDataMember] public DailyTaskEventInfo Info => ActivableInfo;
		[IgnoreDataMember] public IMetaActivableConfigData<EventId> EventInfo => ActivableInfo;
		[IgnoreDataMember] public string Icon => Info.Icon;
		[IgnoreDataMember] public MetaActivableParams MetaActivableParams => ActivableInfo.ActivableParams;
		[IgnoreDataMember] public int VisualizationOrder => ActivableInfo.VisualizationOrder;
		[IgnoreDataMember] public EventAdMode AdMode => ActivableInfo.AdMode;

		public override string ToString() {
			return
				$"{ActivableId}, Level {Level}/{Info.Rewards.Count - 1}, Tasks\n  {String.Join(",\n  ", Tasks.AsEnumerable().Select(task => $"[{task.ToString()}]").ToArray())}";
		}

		public bool AdSeen => AdSeenTime > MetaTime.Epoch;

		public DailyTaskEventModel() { }

		public DailyTaskEventModel(DailyTaskEventInfo info) : base(info) {
			Level = 0;
			Tasks = new List<DailyTaskItem>();
		}

		protected override void OnStartedActivation(IPlayerModelBase player) {
			PlayerModel playerModel = (PlayerModel)player;
			if (!Terminated) {
				foreach (DailyTaskItem taskItem in Tasks) {
					if (taskItem.Completed &&
						!taskItem.RewardClaimed) {
						taskItem.ClaimReward(playerModel);
					}
				}

				if (Completed()) {
					GiveMainReward(playerModel);
				}
			}

			int maxLevel = Info.Rewards.Count - 1;
			if (Completed()) {
				AppliedLevelPenalties = 0;
				Level++;
				if (Level > maxLevel) {
					Level = 0;
				}
			} else {
				if (AppliedLevelPenalties < Info.LevelPenaltyRepeats) {
					AppliedLevelPenalties++;
					Level -= Info.LevelPenalty;
					if (Level < 0) {
						Level = 0;
					}
				}
			}

			MetaScheduleOccasion startingOccasion = Info.ActivableParams.Schedule
				.TryGetCurrentOrNextEnabledOccasion(playerModel.GetCurrentLocalTime()).Value;
			MetaTime startTime = startingOccasion.EnabledRange.Start + LatestActivation.Value.UtcOffset;
			Reset(startTime, playerModel.Random, playerModel);
			playerModel.ClientListener.OnEventStateChanged(ActivableId);
		}

		protected override void Finalize(IPlayerModelBase player) {
			if (Info.ReshowAd) {
				AdSeenTime = MetaTime.Epoch;
			}

			((PlayerModel)player).ClientListener.OnEventStateChanged(ActivableId);
		}

		public void Progress(
			DailyTaskTypeId taskType,
			int amount,
			IPlayerModelClientListener listener,
			ResourceModificationContext context
		) {
			foreach (DailyTaskItem taskItem in Tasks) {
				if (taskItem.TaskInfo.Type == taskType && !taskItem.Completed) {
					int actualProgressAmount = taskItem.Progress(amount);
					listener.OnDailyTaskProgressMade(ActivableId, actualProgressAmount, context);
				}
			}
		}

		public void GiveMainReward(PlayerModel player) {
			ItemCountInfo rewardItem = new ItemCountInfo(Info.Rewards[Level].Type, Info.Rewards[Level].Level, 1);
			RewardMetadata metadata = new RewardMetadata {
				Type = RewardType.DailyTaskAutoClaim,
				Level = Level,
				Event = ActivableId
			};
			RewardModel rewardModel = new RewardModel(
				new List<ResourceInfo>(),
				new List<ItemCountInfo> { rewardItem },
				ChainTypeId.LevelUpRewards,
				1,
				metadata
			);
			player.AddReward(rewardModel);
			player.Logbook.RegisterTaskProgress(
				LogbookTaskType.DailyTasksComplete,
				player.CurrentTime,
				player.ClientListener
			);
		}

		public bool Completed() {
			foreach (DailyTaskItem taskItem in Tasks) {
				if (!taskItem.Completed) {
					return false;
				}
			}

			// Tasks is empty upon the first OnStartedActivation call.
			return Tasks.Count > 0;
		}

		public int CompletedCount {
			get {
				int total = 0;
				foreach (DailyTaskItem task in Tasks) {
					if (task.Completed) {
						total++;
					}
				}

				return total;
			}
		}

		public void ClaimReward(PlayerModel player, int slot) {
			Tasks[slot].ClaimReward(player);
			if (Completed() &&
				UnclaimedRewards() == 0) {
				GiveMainReward(player);
				Terminate();
			}
		}

		public int ClaimedRewards() {
			int claimedRewards = 0;
			foreach (DailyTaskItem taskItem in Tasks) {
				if (taskItem.RewardClaimed) {
					claimedRewards++;
				}
			}

			return claimedRewards;
		}

		public int UnclaimedRewards() {
			int unclaimedRewards = 0;
			foreach (DailyTaskItem taskItem in Tasks) {
				if (taskItem.Completed && !taskItem.RewardClaimed) {
					unclaimedRewards++;
				}
			}
			return unclaimedRewards;
		}

		private void Reset(MetaTime startTime, RandomPCG random, PlayerModel player) {
			Terminated = false;
			StartTime = startTime;
			Tasks.Clear();

			List<DailyTaskSlotAlternativesInfo> taskAlternativeLists =
				player.GameConfig.DailyTasksById[Info.DailyTaskSetId];
			foreach (DailyTaskSlotAlternativesInfo taskAlternativesForSlot in taskAlternativeLists) {
				int chosenTaskIndex = random.NextInt(taskAlternativesForSlot.Tasks.Count);
				DailyTaskInfo chosenTaskInfo = taskAlternativesForSlot.Tasks[chosenTaskIndex];
				DailyTaskItem task = new DailyTaskItem(chosenTaskInfo, taskAlternativesForSlot.Slot);
				Tasks.Add(task);
			}
		}

		public void MarkAdSeen(MetaTime time) {
			AdSeenTime = time;
		}

		public void Terminate() {
			Terminated = true;
		}
	}

	[MetaSerializable]
	public class DailyTaskItem {
		[MetaMember(1)] public DailyTaskInfo TaskInfo { get; private set; }
		[MetaMember(2)] public int Slot { get; private set; }
		[MetaMember(3)] public int CompletedAmount { get; private set; }
		[MetaMember(4)] public bool RewardClaimed { get; private set; }

		public bool Completed => CompletedAmount == TaskInfo.Amount;

		public DailyTaskItem() { }

		public DailyTaskItem(DailyTaskInfo taskInfo, int slot) {
			TaskInfo = taskInfo;
			Slot = slot;
		}

		/// <summary>
		/// <c>Progress</c> progresses the task with the given amount and returns the actual progress made (which
		/// can be smaller than <paramref name="amount"/> if the task is close to be completed.
		/// </summary>
		/// <param name="amount">how much to progress the task</param>
		/// <returns>how much progress was made</returns>
		public int Progress(int amount) {
			int actualProgress = Math.Min(amount, TaskInfo.Amount - CompletedAmount);
			CompletedAmount += actualProgress;
			return actualProgress;
		}

		public void ClaimReward(PlayerModel player) {
			ResourceInfo reward = TaskInfo.Reward;
			player.EarnResources(
				reward.Type,
				reward.Amount,
				IslandTypeId.None,
				new DailyTaskResourceContext(TaskInfo, Slot)
			);
			RewardClaimed = true;
			player.Logbook.RegisterTaskProgress(LogbookTaskType.DailyTask, player.CurrentTime, player.ClientListener);
		}

		public override string ToString() {
			return $"{Slot}:{TaskInfo.Type} {CompletedAmount}/{TaskInfo.Amount}";
		}
	}
}
