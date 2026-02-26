using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;
using System.Collections.Generic;
using UniRx;

namespace Code.Logbook {
	public delegate void TaskModifiedDelegate(LogbookTaskId taskId);
	public delegate void ChapterUnlockedDelegate(LogbookChapterId chapterId);
	public delegate void ChapterModifiedDelegate(LogbookChapterId chapterId);

	public interface ILogbookTasksController {
		IReadOnlyReactiveProperty<bool> HasPendingRewards { get; }

		IEnumerable<LogbookChapterModel> GetChapters();
		void ClaimTaskReward(LogbookTaskId taskId);
		void ClaimChapterReward(LogbookChapterId chapterId);
		void NotifyChapterOpened(LogbookChapterId chapterId);
		void OnLogbookTaskModified(LogbookTaskId taskId);
		void OnLogbookChapterUnlocked(LogbookChapterId chapterId);
		void OnLogbookChapterModified(LogbookChapterId chapterId);

		event TaskModifiedDelegate TaskModified;
		event ChapterUnlockedDelegate ChapterUnlocked;
		event ChapterModifiedDelegate ChapterModified;
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class LogbookTasksController : ILogbookTasksController {
		private readonly ReactiveProperty<bool> hasPendingRewards;

		public LogbookTasksController() {
			hasPendingRewards = new ReactiveProperty<bool>(HasPendingLogbookTasksRewards());
		}

		public IReadOnlyReactiveProperty<bool> HasPendingRewards => hasPendingRewards;

		public IEnumerable<LogbookChapterModel> GetChapters() {
			foreach (
                MetaDictionary<LogbookChapterId, LogbookChapterModel>.KeyValue kvp
				in MetaplayClient.PlayerModel.Logbook.Chapters
			) {
				yield return kvp.Value;
			}
		}

		public void ClaimTaskReward(LogbookTaskId taskId) {
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerClaimLogbookTaskReward(taskId));
		}

		public void ClaimChapterReward(LogbookChapterId chapterId) {
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerClaimLogbookChapterReward(chapterId));
		}

		public void NotifyChapterOpened(LogbookChapterId chapterId) {
			LogbookChapterModel chapter = MetaplayClient.PlayerModel.Logbook.Chapters[chapterId];
			if (chapter.State == ChapterState.Opening) {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerOpenLogbookChapter(chapterId));
			}
		}

		public void OnLogbookTaskModified(LogbookTaskId taskId) {
			UpdateHasPendingLogbookTasksRewards();
			TaskModified?.Invoke(taskId);
		}

		public void OnLogbookChapterUnlocked(LogbookChapterId chapterId) {
			UpdateHasPendingLogbookTasksRewards();
			ChapterUnlocked?.Invoke(chapterId);
		}

		public void OnLogbookChapterModified(LogbookChapterId chapterId) {
			UpdateHasPendingLogbookTasksRewards();
			ChapterModified?.Invoke(chapterId);
		}

		public event TaskModifiedDelegate TaskModified;
		public event ChapterUnlockedDelegate ChapterUnlocked;
		public event ChapterModifiedDelegate ChapterModified;

		private bool HasPendingLogbookTasksRewards() {
			foreach (LogbookChapterModel chapter in GetChapters()) {
				if (chapter.State == ChapterState.Complete) {
					return true;
				}

				foreach (LogbookTaskModel task in chapter.Tasks.Values) {
					if (task.IsComplete &&
						!task.IsClaimed) {
						return true;
					}
				}
			}

			return false;
		}

		private void UpdateHasPendingLogbookTasksRewards() {
			hasPendingRewards.Value = HasPendingLogbookTasksRewards();
		}
	}
}
