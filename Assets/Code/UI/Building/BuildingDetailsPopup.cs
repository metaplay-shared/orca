using System.Threading;
using Code.Purchasing;
using Code.UI.Application;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Building {
	public class BuildingDetailsPopup : UIRootBase<BuildingPopupPayload> {
		[SerializeField] private Button CloseButton;

		[SerializeField] private TMP_Text Title;
		[SerializeField] private Image Icon;
		[SerializeField] private TMP_Text TimeLabel;
		[SerializeField] private CurrencyLabel SkipCostLabel;

		[Inject] private AddressableManager addressableManager;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IPurchasingFlowController purchasingFlowController;

		protected override void Init() {
			int maxLevel = MetaplayClient.PlayerModel.GameConfig.ChainMaxLevels.GetMaxLevel(UIHandle.BuildingType);
			ChainInfo chainInfo =
				MetaplayClient.PlayerModel.GameConfig.Chains[new LevelId<ChainTypeId>(UIHandle.BuildingType, maxLevel)];
			Title.text = Localizer.Localize($"Chain.{UIHandle.BuildingType}.{maxLevel}");
			Icon.sprite = addressableManager.GetItemIcon(chainInfo);
		}

		protected override async UniTask Idle(CancellationToken ct) {
			await UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}

		public void OnCloseClicked() {
			CloseButton.onClick.Invoke();
		}

		public void OnSkipClicked() {
			IslandModel island = MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value];
			Cost cost = island.SkipCreatorTimerCost(
				MetaplayClient.PlayerModel.GameConfig,
				MetaplayClient.PlayerModel.CurrentTime
			);

			purchasingFlowController.TrySpendGemsAsync(cost.Amount, CancellationToken.None)
				.ContinueWith(
					success => {
						if (success) {
							MetaplayClient.PlayerContext.ExecuteAction(
								new PlayerSkipBuildingDailyTimer(island.Info.Type)
							);
						}
					}
				).Forget();
		}

		private bool closed = false;
		private float nextUpdate = 0;

		private void Update() {
			if (closed) {
				return;
			}

			if (Time.time >= nextUpdate) {
				IslandModel island = MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value];
				MetaDuration timeToNext = island.TimeToDailyRewards(
					MetaplayClient.PlayerModel.GameConfig,
					MetaplayClient.PlayerModel.CurrentTime
				);

				if (timeToNext == MetaDuration.Zero) {
					OnCloseClicked();
					closed = true;
					return;
				}

				TimeLabel.text = timeToNext.ToSimplifiedString();

				Cost cost = island.SkipCreatorTimerCost(
					MetaplayClient.PlayerModel.GameConfig,
					MetaplayClient.PlayerModel.CurrentTime
				);
				SkipCostLabel.Set(cost.Type, cost.Amount);
				nextUpdate += 1;
			}
		}
	}
}
