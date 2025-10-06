using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class IslandTasksModel {
		[MetaMember(1)] public MetaDictionary<IslanderId, IslandTaskModel> Tasks { get; private set; }
		[MetaMember(2)] public MetaDictionary<IslanderId, int> TaskIds { get; private set; }
		[MetaMember(3)] public RandomPCG Random { get; private set; }

		public IslandTasksModel() { }

		public IslandTasksModel(MetaTime currentTime) {
			Tasks = new MetaDictionary<IslanderId, IslandTaskModel>();
			TaskIds = new MetaDictionary<IslanderId, int>();
			Random = RandomPCG.CreateFromSeed((ulong) currentTime.MillisecondsSinceEpoch);
		}

		public bool IsItemUsed(LevelId<ChainTypeId> id) {
			foreach (IslandTaskModel task in Tasks.Values) {
				if (task.IsItemUsed(id)) {
					return true;
				}
			}

			return false;
		}
		
		public int GetHighestUsedItemLevel(ChainTypeId id) {
			int highestLevel = -1;
			foreach (IslandTaskModel task in Tasks.Values) {
				var usedItemLevel = task.GetUsedItemLevel(id);
				if (usedItemLevel > highestLevel) {
					highestLevel = usedItemLevel;
				}
			}

			return highestLevel;
		}

		public int CompleteTask(IslanderId islander) {
			IslandTaskModel task = Tasks[islander];
			Tasks.Remove(islander);
			TaskIds[islander] = task.Info.Id;
			return task.Info.Id;
		}

		public void UpdateTasks(IslandInfo info, SharedGameConfig gameConfig, IPlayerModelClientListener listener) {
			foreach (IslanderId islander in info.Islanders) {
				if (Tasks.ContainsKey(islander)) {
					IslandTaskModel task = Tasks[islander];
					if (!task.Enabled) {
						task.Enabled = IsTaskEnabled(task.Info);
						if (task.Enabled) {
							listener.OnIslandTaskModified(info.Type, islander);
						}
					}
				} else {
					IslandTaskInfo taskInfo = NextTask(gameConfig, islander, info.TaskLooping);
					if (taskInfo != null) {
						Tasks[islander] = new IslandTaskModel(taskInfo) {
							Enabled = IsTaskEnabled(taskInfo)
						};
					}
					listener.OnIslandTaskModified(info.Type, islander);
				}
			}
		}

		private IslandTaskInfo NextTask(SharedGameConfig gameConfig, IslanderId islander, bool loopEnabled) {
			int maxId = gameConfig.MaxIslandTaskIds.GetValueOrDefault(islander);
			if (maxId == 0) {
				return null;
			}

			int completedId = TaskIds.GetValueOrDefault(islander);
			if (completedId < maxId) {
				for (int i = completedId + 1; i <= maxId; i++) {
					LevelId<IslanderId> taskId = new(islander, i);
					if (gameConfig.IslandTasks.ContainsKey(taskId)) {
						return gameConfig.IslandTasks[taskId];
					}
				}
			} else if (loopEnabled) {
				IslandTaskInfo taskInfo = RandomTask(gameConfig, islander, maxId);
				if (taskInfo == null) {
					return null;
				}

				int currentId = taskInfo.Id - 1;
				while (currentId > 0) {
					LevelId<IslanderId> taskId = new(islander, currentId);
					if (gameConfig.IslandTasks.ContainsKey(taskId)) {
						IslandTaskInfo previousTask = gameConfig.IslandTasks[taskId];
						if (previousTask.GroupId != taskInfo.GroupId) {
							return taskInfo;
						}

						taskInfo = previousTask;
					}

					currentId--;
				}

				return taskInfo;
			}

			return null;
		}

		private IslandTaskInfo RandomTask(SharedGameConfig gameConfig, IslanderId islander, int maxId) {
			LevelId<IslanderId> taskId = new(islander, Random.NextInt(maxId));
			if (gameConfig.IslandTasks.ContainsKey(taskId)) {
				return gameConfig.IslandTasks[taskId];
			}
			for (int i = 1; i < maxId; i++) {
				taskId = new(islander, i);
				if (gameConfig.IslandTasks.ContainsKey(taskId)) {
					return gameConfig.IslandTasks[taskId];
				}
			}
			return null;
		}

		private bool IsTaskEnabled(IslandTaskInfo taskInfo) {
			foreach (LevelId<IslanderId> dependency in taskInfo.Dependencies) {
				int lastId = TaskIds.GetValueOrDefault(dependency.Type);
				if (lastId < dependency.Level) {
					return false;
				}
			}

			return true;
		}
	}
}
