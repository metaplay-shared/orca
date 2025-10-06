using Code.UI.AssetManagement;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.Hero {
	public class HeroTaskReward : MonoBehaviour {
		[SerializeField] private Image Icon;
		[SerializeField] private TMP_Text CountLabel;
		[Inject] private AddressableManager addressableManager;

		public void Setup(ItemCountInfo reward) {
			Icon.sprite = addressableManager.GetItemIcon(reward.Type, reward.Level);
			CountLabel.text = $"x{reward.Count}";
			CountLabel.gameObject.SetActive(reward.Count > 1);
		}
	}
}
