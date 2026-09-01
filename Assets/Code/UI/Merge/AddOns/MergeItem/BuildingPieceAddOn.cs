using Code.UI.Application;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class BuildingPieceAddOn : ItemAddOn {
		[SerializeField] private GameObject BuildingPieceReadyIndicator;

		[Inject] private ApplicationInfo applicationInfo;

		protected override void Setup() {
			OnStateChanged();
		}

		public override void OnStateChanged() {
			bool shouldHighlight = MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value].CanClaimAsBuilding(
				MetaplayClient.PlayerModel.GameConfig,
				ItemModel
			);

			BuildingPieceReadyIndicator.SetActive(shouldHighlight);
		}
	}
}
