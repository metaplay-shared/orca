using Code.UI.Application;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class BuildingLevelAddOn : ItemAddOn {
		[SerializeField] private Image[] LevelStars;
		[SerializeField] private RectTransform Root;
		[Inject] private ApplicationInfo applicationInfo;

		public override bool IsActive =>
			ItemModel?.Info.Building == true &&
			MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value].BuildingState == BuildingState.Complete;

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

			IslandModel island = MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value];
			for (int i = 0; i < LevelStars.Length; i++) {
				if (i < island.BuildingLevel.Level) {
					LevelStars[i].fillAmount = 1;
				} else if (i == island.BuildingLevel.Level) {
					LevelStars[i].fillAmount = (float) island.BuildingLevel.CurrentXp /
						island.BuildingLevel.GetLevelInfo(MetaplayClient.PlayerModel.GameConfig).XpToNextLevel;
				} else {
					LevelStars[i].fillAmount = 0;
				}
			}
		}
	}
}
