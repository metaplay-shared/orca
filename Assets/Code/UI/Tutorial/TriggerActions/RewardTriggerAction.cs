using System.Threading;
using Code.UI.Rewarding;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Code.UI.Tutorial.TriggerActions {
	public class RewardTriggerAction : TriggerAction {
		[Inject] private IRewardController rewardController;

		public override async UniTask Run( /*TODO: CancellationToken ct*/) {
			// HACK: Using a none cancellation token for now.
			// TODO: Make cancellation tokens required part of the Run signature.
			CancellationToken ct = CancellationToken.None;
			// HACK: This is called from other actions and we have to wait a frame to make sure
			// the execution of that action is done. Show rewards will execute another action and starting
			// actions within other actions in not allowed.
			// TODO: Unify claiming rewards in a way that allows other components not to care about this detail.
			await UniTask.Yield(ct);
			await rewardController.ShowRewards(ct);
		}
	}
}
