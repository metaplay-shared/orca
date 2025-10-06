using System.Threading;
using Code.UI.Application;
using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Unity.DefaultIntegration;
using Zenject;

namespace Code.UI.Rewarding {
	public interface IRewardController {
		UniTask ShowRewards(CancellationToken ct);
	}

	[UsedImplicitly]
	public class RewardController : IRewardController {
		[Inject] private IUIRootController uiRootController;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private SignalBus signalBus;

		public async UniTask ShowRewards(CancellationToken ct) {
			signalBus.Fire(new RewardShownSignal());

			while (MetaplayClient.PlayerModel.Rewards.Count != 0) {
				RewardModel playerModelReward = MetaplayClient.PlayerModel.Rewards[0];

				await uiRootController.ShowUI<RewardPopup, RewardPopupPayload>(new(playerModelReward), ct).OnComplete;

				// Actual claiming happens when the popup is closed for two reasons:
				// - We want to show the effect of claiming in the resource counters
				// - Claiming can trigger other actions and flows and we do not want
				//   these to conflict with the reward flow

				IslandTypeId island = applicationInfo.ActiveIsland.Value == null
					? IslandTypeId.None
					: applicationInfo.ActiveIsland.Value;

				// This will remove the reward from Rewards collection
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerClaimReward(island));
			}
		}
	}
}
