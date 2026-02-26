using Code.Logbook;
using Game.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Code.UI.Logbook.Tasks {
	public class LogbookTasksPresenter : MonoBehaviour {
		[SerializeField] private LogbookTaskCardPresenter TemplateTaskCard;
		[SerializeField] private Transform TaskCardsContainer;

		[Inject] private DiContainer container;
		[Inject] private ILogbookTasksController logbookTasksController;

		private readonly Dictionary<LogbookTaskId, LogbookTaskCardPresenter> taskCards = new();
		private LogbookChapterModel chapterModel;

		private void OnEnable() {
			logbookTasksController.TaskModified += HandleTaskModified;
		}

		private void OnDisable() {
			logbookTasksController.TaskModified -= HandleTaskModified;
		}

		private void HandleTaskModified(LogbookTaskId taskId) {
			ReorderTaskCards();
		}

		private void ReorderTaskCards() {
			List<LogbookTaskModel> taskModels = chapterModel.Tasks.Values.ToList();
			taskModels.Sort(CompareTaskModels);

			for (int i = 0; i < taskModels.Count; i++) {
				LogbookTaskModel taskModel = taskModels[i];
				LogbookTaskCardPresenter presenter = taskCards[taskModel.Info.Id];
				presenter.transform.SetSiblingIndex(i);
			}
		}

		public void Setup(LogbookChapterModel chapter) {
			chapterModel = chapter;

			foreach (Transform child in TaskCardsContainer) {
				Destroy(child.gameObject);
			}
			taskCards.Clear();

			List<LogbookTaskModel> taskModels = chapter.Tasks.Values.ToList();
			taskModels.Sort(CompareTaskModels);
			foreach (LogbookTaskModel taskModel in taskModels) {
				DiContainer taskContainer = container.CreateSubContainer();
				taskContainer.BindInstance(taskModel).AsSingle();
				LogbookTaskCardPresenter taskCardPresenter =
					taskContainer.InstantiatePrefabForComponent<LogbookTaskCardPresenter>(
						TemplateTaskCard,
						TaskCardsContainer
					);
				taskCards.Add(taskModel.Info.Id, taskCardPresenter);
			}
		}

		private int CompareTaskModels(LogbookTaskModel a, LogbookTaskModel b) {
			int activeDiff = Convert.ToInt32(IsCompleteAndNotClaimed(b)) - Convert.ToInt32(IsCompleteAndNotClaimed(a));
			if (activeDiff != 0) {
				return activeDiff;
			}

			int claimedDiff = Convert.ToInt32(a.IsClaimed) - Convert.ToInt32(b.IsClaimed);
			if (claimedDiff != 0) {
				return claimedDiff;
			}

			int isOpenDiff = Convert.ToInt32(IsOpen(b)) - Convert.ToInt32(IsOpen(a));
			if (isOpenDiff != 0) {
				return isOpenDiff;
			}

			return 0;

			static bool IsCompleteAndNotClaimed(LogbookTaskModel task) => task.IsComplete && !task.IsClaimed;
			static bool IsOpen(LogbookTaskModel task) => task.IsOpen;
		}
	}
}
