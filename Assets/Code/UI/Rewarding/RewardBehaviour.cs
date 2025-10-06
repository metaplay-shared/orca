using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Code.UI.Rewarding {
	public class RewardBehaviour : MonoBehaviour {
		private bool rewardsInQueue;

		[Inject] private IRewardController rewardController;

		public void EnqueueReward() {
			// TODO: This enqueue could be replaced with waiting for rewardController.ShowRewards
			rewardsInQueue = true;
		}

		private void Update() {
			if (!rewardsInQueue) {
				return;
			}

			rewardsInQueue = false;

			rewardController.ShowRewards(this.GetCancellationTokenOnDestroy()).Forget();
		}
	}
}
