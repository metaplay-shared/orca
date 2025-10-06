using Code.UI.RequirementsDisplay;
using UnityEngine;
using Zenject;

namespace Code.UI.Tasks {
	public class TaskDetails : MonoBehaviour {
		[SerializeField] protected CompleteTaskButton CompleteTaskButton;
		[SerializeField] protected ClaimRewardButton ClaimButton;
		[SerializeField] protected ResourceRequirements ResourceRequirements;

		[Inject] protected SignalBus signalBus;
	}
}
