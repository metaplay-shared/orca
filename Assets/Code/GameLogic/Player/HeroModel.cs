using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class HeroModel {
		[MetaMember(1)] public HeroInfo Info { get; private set; }
		[MetaMember(2)] public MetaTime UnlockedAt { get; private set; }
		[MetaMember(3)] public HeroLevelModel Level { get; private set; }
		[MetaMember(4)] public HeroTaskModel CurrentTask { get; private set; }
		[MetaMember(5)] public int MaxCompletedId { get; private set; }
		[MetaMember(6)] public RandomPCG Random { get; private set; }
		[MetaMember(7)] public ChainTypeId Building { get; private set; }

		public HeroModel() {
		}

		public HeroModel(HeroInfo info, MetaTime currentTime) {
			Info = info;
			UnlockedAt = currentTime;
			Level = new HeroLevelModel(Info.Type);
			MaxCompletedId = 0;
			Random = RandomPCG.CreateFromSeed((ulong) currentTime.MillisecondsSinceEpoch);
		}

		public void AssignToBuilding(ChainTypeId building) {
			Building = building;
		}

		public void AddXp(SharedGameConfig gameConfig, int delta, Action<RewardModel> rewardHandler, IPlayerModelClientListener listener, IPlayerModelServerListener serverListener) {
			Level.AddXp(gameConfig, delta, rewardHandler, listener, serverListener, ResourceModificationContext.Empty);
		}

		public void Update(SharedGameConfig gameConfig, int playerLevel, OrderedSet<ChainTypeId> unlockedResources, MetaTime currentTime, IPlayerModelClientListener listener) {
			if (CurrentTask == null ||
				(CurrentTask.State == HeroTaskState.Claimed &&
					CurrentTask.ClaimedAt + Info.TaskCooldown <= currentTime)) {
				HeroTaskInfo taskInfo = NextTask(gameConfig, playerLevel, unlockedResources);
				if (taskInfo != null) {
					CurrentTask = new HeroTaskModel(taskInfo, currentTime);
					listener.OnHeroTaskModified(Info.Type);
				}
			} else if (CurrentTask.State == HeroTaskState.Created && CurrentTask.Info.GoldenTask &&
				CurrentTask.CreatedAt + gameConfig.Global.GoldenHeroTaskTtl <= currentTime) {
				HeroTaskInfo taskInfo = NextTask(gameConfig, playerLevel, unlockedResources);
				if (taskInfo != null) {
					CurrentTask = new HeroTaskModel(taskInfo, currentTime);
					listener.OnHeroTaskModified(Info.Type);
				}
			} else {
				if (CurrentTask.State == HeroTaskState.Fulfilled && CurrentTask.FinishedAt <= currentTime) {
					CurrentTask.Finish();
					listener.OnHeroTaskModified(Info.Type);
				}
			}
		}

		public void FulfillTask(MetaTime currentTime) {
			CurrentTask.Fulfill(currentTime);
			MaxCompletedId = Math.Max(MaxCompletedId, CurrentTask.Info.Id);
		}

		private HeroTaskInfo NextTask(SharedGameConfig gameConfig, int playerLevel, OrderedSet<ChainTypeId> unlockedResources) {
			int maxId = MaxTaskId(gameConfig, playerLevel, unlockedResources);
			if (maxId == 0) {
				return null;
			}

			if (MaxCompletedId < maxId) {
				// The last task not completed yet -> pick tasks in sequence
				for (int i = MaxCompletedId + 1; i <= maxId; i++) {
					if (gameConfig.HeroTasks.ContainsKey(i)) {
						HeroTaskInfo task = gameConfig.HeroTasks[i];
						if (IsEligible(gameConfig, task, playerLevel, unlockedResources) && task.RunInSequence) {
							return task;
						}
					}
				}
			}

			// Also the last task completed at least once -> choose a random task
			HeroLevelInfo levelInfo = gameConfig.HeroLevels[new LevelId<HeroTypeId>(Info.Type, Level.Level)];
			F64 randomValue = F64.FromInt(Random.NextInt(10000)) / 10000;
			List<HeroTaskInfo> taskCollection = gameConfig.GoldenHeroTasks.Count > 0 && randomValue < levelInfo.GoldenTaskProbability
				? gameConfig.GoldenHeroTasks
				: gameConfig.NormalHeroTasks;
			if (taskCollection.Count == 0) {
				return null;
			}

			for (int i = 0; i < 100; i++) {
				HeroTaskInfo task = taskCollection[Random.NextInt(taskCollection.Count)];
				if (IsEligible(gameConfig, task, playerLevel, unlockedResources)) {
					return task;
				}
			}


			return null;
		}

		private int MaxTaskId(SharedGameConfig gameConfig, int playerLevel, OrderedSet<ChainTypeId> unlockedResources) {
			int maxId = 0;
			foreach (int taskId in gameConfig.HeroTasks.Keys) {
				HeroTaskInfo taskInfo = gameConfig.HeroTasks[taskId];
				if (IsEligible(gameConfig, taskInfo, playerLevel, unlockedResources) && taskId > maxId) {
					maxId = taskId;
				}
			}

			return maxId;
		}

		private bool IsEligible(SharedGameConfig gameConfig, HeroTaskInfo task, int playerLevel, OrderedSet<ChainTypeId> unlockedResources) {
			foreach (ResourceInfo resource in task.Resources) {
				ChainTypeId chainType = gameConfig.ResourceItems[resource.Type];
				if (!unlockedResources.Contains(chainType)) {
					return false;
				}
			}

			// TODO: Remove the old handling once the config is fully migrated
			if (task.PlayerLevel > 0) {
				return task.PlayerLevel <= playerLevel && task.HeroLevel <= Level.Level && task.Building == Building;
			}

			return playerLevel >= task.MinPlayerLevel &&
				playerLevel <= task.MaxPlayerLevel &&
				Level.Level >= task.MinHeroLevel &&
				Level.Level <= task.MaxHeroLevel &&
				task.Building == Building;
		}
	}

	[MetaSerializable]
	public class HeroTaskModel {
		[MetaMember(1)] public HeroTaskInfo Info { get; private set; }
		[MetaMember(2)] public HeroTaskState State { get; private set; }
		[MetaMember(3)] public MetaTime CreatedAt { get; private set; }
		[MetaMember(4)] public MetaTime FulfilledAt { get; private set; }
		[MetaMember(5)] public MetaTime FinishedAt { get; private set; }
		[MetaMember(6)] public MetaTime ClaimedAt { get; private set; }

		public HeroTaskModel() {
		}

		public HeroTaskModel(HeroTaskInfo info, MetaTime currentTime) {
			State = HeroTaskState.Created;
			Info = info;
			CreatedAt = currentTime;
			FulfilledAt = MetaTime.Epoch;
			FinishedAt = MetaTime.Epoch;
			ClaimedAt = MetaTime.Epoch;
		}

		public void Fulfill(MetaTime currentTime) {
			State = HeroTaskState.Fulfilled;
			FulfilledAt = currentTime;
			FinishedAt = currentTime + Info.CompletionTime;
		}

		public void Finish() {
			State = HeroTaskState.Finished;
		}

		public void FinishNow(MetaTime currentTime) {
			State = HeroTaskState.Finished;
			FinishedAt = currentTime;
		}

		public void Claim(MetaTime currentTime) {
			State = HeroTaskState.Claimed;
			ClaimedAt = currentTime;
		}
	}

	[MetaSerializable]
	public enum HeroTaskState {
		Created,
		Fulfilled,
		Finished,
		Claimed
	}
}
