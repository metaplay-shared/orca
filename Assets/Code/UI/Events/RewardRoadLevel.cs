using Code.UI.Application;
using Code.UI.HudBase;
using Code.UI.Merge;
using Cysharp.Threading.Tasks;
using Game.Logic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events {
	public class RewardRoadLevel : MonoBehaviour {
		[SerializeField] private RewardRoadItem FreeItem;
		[SerializeField] private RewardRoadItem PremiumItem;
		public int Level { get; private set; }
		private bool canBeClaimed;
		[Inject] private ApplicationInfo applicationInfo;

		public void Setup(ActivityEventModel model, ActivityEventLevelInfo level) {
			Level = level.Level;
			bool unlocked = model.EventLevel.Level >= level.Level;
			bool premiumPassPurchased = model.PremiumPassPurchase.HasValue;
			bool freeClaimed = model.ClaimedRewardsFree.ContainsKey(level.Level);
			bool premiumClaimed = model.ClaimedRewardsPremium.ContainsKey(level.Level);

			FreeItem.Setup(level.EventId, level.FreeRewardItem, unlocked, freeClaimed, true);
			PremiumItem.Setup(
				level.EventId,
				level.PremiumRewardItem,
				unlocked && premiumPassPurchased,
				premiumClaimed,
				premiumPassPurchased
			);
		}

		public void FlyRewards(bool premium) {
			bool mainIsland = applicationInfo.ActiveIsland.Value == IslandTypeId.MainIsland;
			if (premium) {
				PremiumItem.FlyObject(mainIsland).Forget();
			} else {
				FreeItem.FlyObject(mainIsland).Forget();
			}
		}
	}
}
