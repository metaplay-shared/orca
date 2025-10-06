using Code.UI.AssetManagement;
using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeBoard.LockArea {
	public class LockAreaDetails : MonoBehaviour {
		[SerializeField] private TMP_Text PlayerLevel;
		[SerializeField] private Image HeroIcon;
		[SerializeField] private TMP_Text HeroLevel;

		[Inject] private AddressableManager addressableManager;

		public void Setup(int playerLevel, HeroTypeId hero, int heroLevel) {
			PlayerLevel.text = playerLevel.ToString();
			if (hero == HeroTypeId.None) {
				HeroIcon.gameObject.SetActive(false);
			} else if (MetaplayClient.PlayerModel.GameConfig.Heroes.ContainsKey(hero)) {
				ChainTypeId itemType = MetaplayClient.PlayerModel.GameConfig.Heroes[hero].ItemType;
				int level = MetaplayClient.PlayerModel.GameConfig.ChainMaxLevels.GetMaxLevel(itemType);
				var itemId = new LevelId<ChainTypeId>(itemType, level);
				if (MetaplayClient.PlayerModel.GameConfig.Chains.ContainsKey(itemId)) {
					ChainInfo chainInfo = MetaplayClient.PlayerModel.GameConfig.Chains[itemId];
					Sprite sprite = addressableManager.GetItemIcon(chainInfo);
					HeroIcon.sprite = sprite;
				} else {
					// TODO: Add some placeholder or throw an exception.
					HeroIcon.gameObject.SetActive(false);
				}

				if (heroLevel > 1) {
					HeroLevel.text = heroLevel.ToString();
				} else {
					HeroLevel.gameObject.SetActive(false);
				}
			} else {
				// TODO: Add some placeholder or throw an exception.
				HeroIcon.gameObject.SetActive(false);
			}
		}
	}
}
