using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class MineLevelAddOn : ItemAddOn {
		[SerializeField] private Image[] LevelStars;
		[SerializeField] private RectTransform Root;

		public override bool IsActive =>
			ItemModel?.Mine != null &&
			MetaplayClient.PlayerModel.GameConfig.MineMaxLevels.GetMaxLevel(ItemModel.Mine.Info.Type) > 1 &&
			ItemModel?.State is not ItemState.Hidden or ItemState.PartiallyVisible;

		protected override void Setup() {
			if (!IsActive) {
				return;
			}

			OnStateChanged();
		}

		public override void OnStateChanged() {
			if (!IsActive) {
				return;
			}

			Root.gameObject.SetActive(true);
			Root.localScale = Vector3.one * ItemModel.Info.Width;
			for (int i = 0; i < LevelStars.Length; i++) {
				if (i < ItemModel.Mine.Info.Level) {
					LevelStars[i].fillAmount = 1;
				} else if (i == ItemModel.Mine.Info.Level) {
					LevelStars[i].fillAmount = (float) ItemModel.Mine.RepairCycle / ItemModel.Mine.Info.RepairCycles;
				} else {
					LevelStars[i].fillAmount = 0;
				}
			}
		}
	}
}
