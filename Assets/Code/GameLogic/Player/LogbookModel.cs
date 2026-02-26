using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class LogbookModel {
		[MetaMember(1)] public MetaDictionary<LogbookChapterId, LogbookChapterModel> Chapters { get; private set; } = new();

		public void Refresh(SharedGameConfig gameConfig, MetaTime currentTime, ITaskContext taskContext, IPlayerModelClientListener listener) {
			foreach (LogbookChapterInfo chapterInfo in gameConfig.LogbookChapters.Values) {
				if (!Chapters.ContainsKey(chapterInfo.Id)) {
					Chapters[chapterInfo.Id] = new LogbookChapterModel(chapterInfo);
				}
			}

			bool openChapterFound = false;
			foreach (LogbookChapterModel chapter in Chapters.Values) {
				if (chapter.State == ChapterState.Open) {
					chapter.RefreshTasks(gameConfig, currentTime, taskContext, listener);
					openChapterFound = true;
				}

				if (chapter.State == ChapterState.Opening || chapter.State == ChapterState.Complete) {
					openChapterFound = true;
				}
			}

			if (!openChapterFound) {
				LogbookChapterModel next = FindChapter(ChapterState.Locked);
				if (next != null) {
					next.RefreshTasks(gameConfig, currentTime, taskContext, listener);
				}
			}
		}

		public void RegisterTaskProgress(
			LogbookTaskType type,
			MetaTime currentTime,
			IPlayerModelClientListener listener
		) {
			LogbookChapterModel currentOpenChapter = FindChapter(ChapterState.Open);
			if (currentOpenChapter == null) {
				return;
			}

			foreach (LogbookTaskModel taskModel in currentOpenChapter.Tasks.Values) {
				if (taskModel.Info.Type == type && taskModel.IsOpen) {
					taskModel.Inc(currentTime, listener);
				}
			}
		}

		public void RegisterTaskProgress(
			LogbookTaskType type,
			ChainInfo chain,
			MetaTime currentTime,
			IPlayerModelClientListener listener
		) {
			LogbookChapterModel currentOpenChapter = FindChapter(ChapterState.Open);
			if (currentOpenChapter == null) {
				return;
			}

			LevelId<ChainTypeId> item = chain.ConfigKey;
			foreach (LogbookTaskModel taskModel in currentOpenChapter.Tasks.Values) {
				LogbookTaskInfo taskInfo = taskModel.Info;
				if (taskInfo.Type == type &&
					taskModel.IsOpen &&
					(taskInfo.Item.Type == ChainTypeId.None || taskInfo.Item.Type == item.Type) &&
					(taskInfo.Item.Level == 0 || taskInfo.Item.Level == item.Level)) {
					int increment = 1;
					if (taskInfo.Type == LogbookTaskType.CollectItem && taskInfo.UseValueAsIncrement) {
						increment = chain.CollectableValue;
					}
					taskModel.Inc(currentTime, listener, increment);
				}
			}
		}

		public void RegisterTaskProgress(
			LogbookTaskType type,
			IslandTypeId island,
			MetaTime currentTime,
			IPlayerModelClientListener listener
		) {
			LogbookChapterModel currentOpenChapter = FindChapter(ChapterState.Open);
			if (currentOpenChapter == null) {
				return;
			}
			foreach (LogbookTaskModel taskModel in currentOpenChapter.Tasks.Values) {
				if (taskModel.Info.Type == type && taskModel.IsOpen && island == taskModel.Info.Island) {
					taskModel.Inc(currentTime, listener);
				}
			}
		}

		public void ClaimTaskReward(SharedGameConfig gameConfig, LogbookTaskId id, MetaTime currentTime, ITaskContext taskContext, IPlayerModelClientListener listener) {
			LogbookTaskInfo info = gameConfig.LogbookTasks[id];
			Chapters[info.Chapter].ClaimTaskReward(gameConfig, id, currentTime, taskContext, listener);
		}

		public void ClaimChapterReward(SharedGameConfig gameConfig, LogbookChapterId id, MetaTime currentTime, ITaskContext taskContext, IPlayerModelClientListener listener) {
			LogbookChapterModel chapter = Chapters[id];
			chapter.ClaimChapterReward();
			listener.OnLogbookChapterModified(id);
			LogbookChapterModel nextChapter = FindChapter(ChapterState.Locked);
			if (nextChapter != null) {
				nextChapter.RefreshTasks(gameConfig, currentTime, taskContext, listener);
				listener.OnLogbookChapterUnlocked(nextChapter.Info.Id);
			}
		}

		public void OpenChapter(LogbookChapterId id) {
			Chapters[id].Open();
		}

		/// <summary>
		/// <c>FindChapter</c> finds the chapter with the lowest index with the given state.
		/// </summary>
		/// <param name="chapterState">state of chapter to find</param>
		/// <returns>chapter with the lowest index and the given state or <c>null</c> if none found</returns>
		private LogbookChapterModel FindChapter(ChapterState chapterState) {
			LogbookChapterModel result = null;
			foreach (LogbookChapterModel chapter in Chapters.Values) {
				if (chapter.State == chapterState && (result == null || chapter.Info.Index < result.Info.Index)) {
					result = chapter;
				}
			}

			return result;
		}
	}

	[MetaSerializable]
	public class LogbookTaskModel {
		[MetaMember(1)] public LogbookTaskInfo Info { get; private set; }
		[MetaMember(2)] public int Count { get; private set; }
		[MetaMember(3)] public MetaTime? OpenedAt { get; private set; }
		[MetaMember(4)] public MetaTime? CompletedAt { get; private set; }
		[MetaMember(5)] public MetaTime? ClaimedAt { get; private set; }

		public LogbookTaskModel() { }

		public LogbookTaskModel(LogbookTaskInfo info) {
			Info = info;
			Count = 0;
		}

		public bool IsOpen => OpenedAt.HasValue;
		public bool IsComplete => CompletedAt.HasValue;
		public bool IsClaimed => ClaimedAt.HasValue;

		public void Inc(MetaTime currentTime, IPlayerModelClientListener listener, int increment = 1) {
			if (Count >= Info.Count) {
				return;
			}

			Count += increment;
			if (Count >= Info.Count) {
				CompletedAt = currentTime;
			}
			listener.OnLogbookTaskModified(Info.Id);
		}

		public void Claim(MetaTime currentTime) {
			ClaimedAt = currentTime;
		}

		public void Open(MetaTime currentTime) {
			OpenedAt = currentTime;
		}
	}

	public interface ITaskContext {
		DiscoveryState GetState(LevelId<ChainTypeId> item);
		IslandState GetState(IslandTypeId island);
		void ExecuteTrigger(TriggerId trigger);
	}

	[MetaSerializable]
	public class LogbookChapterModel {
		[MetaMember(1)] public LogbookChapterInfo Info { get; private set; }
		[MetaMember(2)] public ChapterState State { get; private set; }
		[MetaMember(3)] public MetaDictionary<LogbookTaskId, LogbookTaskModel> Tasks { get; private set; } = new();

		public LogbookChapterModel() {}

		public LogbookChapterModel(LogbookChapterInfo info) {
			Info = info;
			State = Info.Index == 1 ? ChapterState.Open : ChapterState.Locked;
		}

		public void RefreshTasks(SharedGameConfig gameConfig, MetaTime currentTime, ITaskContext taskContext, IPlayerModelClientListener listener) {
			if (State == ChapterState.Locked) {
				State = ChapterState.Opening;
			}

			foreach (LogbookTaskInfo taskInfo in gameConfig.LogbookTasksByChapter[Info.Id]) {
				if (!Tasks.ContainsKey(taskInfo.Id)) {
					Tasks[taskInfo.Id] = new LogbookTaskModel(taskInfo);
				}
			}

			foreach (LogbookTaskModel task in Tasks.Values) {
				if (!task.IsOpen) {
					if (DependenciesClaimed(task)) {
						task.Open(currentTime);
						if (task.Info.Type == LogbookTaskType.ItemDiscovery &&
							taskContext.GetState(task.Info.Item) != DiscoveryState.NotDiscovered) {
							task.Inc(currentTime, listener);
						}
						if (task.Info.Type == LogbookTaskType.UnlockIsland &&
							taskContext.GetState(task.Info.Island) == IslandState.Open) {
							task.Inc(currentTime, listener);
						}
						listener.OnLogbookTaskModified(task.Info.Id);
					}
				}
			}
		}

		public void ClaimChapterReward() {
			State = ChapterState.RewardClaimed;
		}

		public void ClaimTaskReward(SharedGameConfig gameConfig, LogbookTaskId id, MetaTime currentTime, ITaskContext taskContext, IPlayerModelClientListener listener) {
			Tasks[id].Claim(currentTime);
			listener.OnLogbookTaskModified(id);
			if (AllTasksClaimed()) {
				State = ChapterState.Complete;
				listener.OnLogbookChapterModified(Info.Id);
				foreach (TriggerId trigger in Info.Triggers) {
					taskContext.ExecuteTrigger(trigger);
				}
			} else {
				RefreshTasks(gameConfig, currentTime, taskContext, listener);
			}
		}

		public void Open() {
			State = ChapterState.Open;
		}

		private bool DependenciesClaimed(LogbookTaskModel task) {
			foreach (LogbookTaskId dependency in task.Info.Dependencies) {
				if (Tasks.GetValueOrDefault(dependency)?.IsClaimed != true) {
					return false;
				}
			}

			return true;
		}

		private bool AllTasksClaimed() {
			foreach (LogbookTaskModel task in Tasks.Values) {
				if (!task.IsClaimed) {
					return false;
				}
			}

			return true;
		}
	}

	[MetaSerializable]
	public enum ChapterState {
		Locked,
		Opening,
		Open,
		Complete,
		RewardClaimed
	}
}
