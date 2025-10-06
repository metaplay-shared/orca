using System.Collections.Generic;
using System.Linq;
using Game.Logic;
using UnityEngine;

namespace Code.UI.Events {
	public class DailyTasksTaskList : MonoBehaviour {
		[SerializeField] private DailyTaskDetails TaskTemplate;
		private List<DailyTaskDetails> tasks = new List<DailyTaskDetails>();

		public void Setup(DailyTaskEventModel model, DailyTasksUIRoot.ICallbacks callbacks) {
			foreach (DailyTaskItem task in model.Tasks) {
				DailyTaskDetails taskDetails = Instantiate(TaskTemplate, transform);
				taskDetails.Setup(model.ActivableId, task, callbacks);
				tasks.Add(taskDetails);
			}
			SortTasks();
		}

		private void SortTasks() {
			tasks = tasks
				.OrderBy(t => t.Model.Completed && !t.Model.RewardClaimed)
				.ThenBy(t => !t.Model.Completed)
				.ThenBy(t => t.Model.Slot)
				.ToList();
			foreach (DailyTaskDetails task in tasks) {
				task.transform.SetSiblingIndex(0);
			}
		}
	}
}
