using Game.Logic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Code.UI.Tasks.Islanders {
	public class TaskGiverProgressionItem : MonoBehaviour {
		[SerializeField] private GameObject DoneState;
		[SerializeField] private GameObject ActiveState;
		[SerializeField] private GameObject InactiveState;
		[SerializeField] private TMP_Text TaskNumber;

		public void Setup(IslandTaskInfo task, IslandTaskModel taskModel, int taskUiIndex) {
			TaskNumber.text = taskUiIndex.ToString(CultureInfo.InvariantCulture);
			UpdateState(task, taskModel);
		}

		private void UpdateState(IslandTaskInfo task, IslandTaskModel taskModel) {
			State state = GetState(task, taskModel);
			DoneState.SetActive(state == State.Done);
			ActiveState.SetActive(state == State.Active);
			InactiveState.SetActive(state == State.Inactive);
		}

		private State GetState(IslandTaskInfo task, IslandTaskModel taskModel) {
			return task.Id < taskModel.Info.Id ? State.Done :
				task.Id == taskModel.Info.Id ? State.Active : State.Inactive;
		}

		private enum State {
			Done,
			Active,
			Inactive
		}
	}
}
