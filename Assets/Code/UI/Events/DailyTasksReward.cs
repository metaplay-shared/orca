using Code.UI.AssetManagement;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events {
	public class DailyTasksReward : MonoBehaviour {
		[SerializeField] private Image RewardIcon;
		[SerializeField] private GameObject ClaimedIcon;
		[SerializeField] private GameObject CurrentIndicator;

		[Inject] private AddressableManager addressableManager;

		public void UpdateVisuals(LevelId<ChainTypeId> reward, bool claimed, bool current) {
			ChainInfo chainInfo = MetaplayClient.PlayerModel.GameConfig.Chains[reward];
			RewardIcon.sprite = addressableManager.GetItemIcon(chainInfo);
			ClaimedIcon.SetActive(claimed);
			CurrentIndicator.SetActive(current && !claimed);
		}
	}
}
