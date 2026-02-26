using System.Collections.Generic;
using System.Threading;
using Code.UI.Application;
using Code.UI.Building;
using Code.UI.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class BuildingActionsAddOn : ItemAddOn {
		[SerializeField] private GameObject NotificationBubble;

		[SerializeField] private RectTransform RewardBubble;
		[SerializeField] private Button ClaimButton;

		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IUIRootController uiRootController;
		
		private float notificationBubbleTimer;

		public override bool IsActive => ItemModel?.Info.Building == true;
		private bool RewardsReady =>
			MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value].BuildingDailyRewardAvailable(
				MetaplayClient.PlayerModel.GameConfig,
				MetaplayClient.PlayerModel.CurrentTime
			);

		protected override void Setup() {
		}

		private void Awake() {
			ClaimButton.onClick.AddListener(OnClaimClicked);
		}

		private void OnClaimClicked() {
			HideBubble(RewardBubble);
			if (!IsActive) {
				return;
			}

			MetaplayClient.PlayerContext.ExecuteAction(
				new PlayerClaimBuildingDailyReward(applicationInfo.ActiveIsland.Value)
			);
		}

		public override void OnStateChanged() {
			if (!IsActive || !RewardsReady) {
				HideBubble(RewardBubble);
			}
		}

		public override void OnDeselected() {
			HideBubble(RewardBubble);
		}

		public override void OnBeginDrag() {
			HideBubble(RewardBubble);
		}

		public override void OnOpen() {
			if (MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value].BuildingState ==
				BuildingState.Complete) {
				if (RewardsReady) {
					ShowBubble(RewardBubble);
				} else {
					List<ChainTypeId> fragments =
						MetaplayClient.PlayerModel.GameConfig.IslandBuildingFragments[applicationInfo.ActiveIsland.Value];
					ChainTypeId type = fragments[0];

					uiRootController.ShowUI<BuildingDetailsPopup, BuildingPopupPayload>(
						new BuildingPopupPayload(type, ItemModel.Info.Type),
						CancellationToken.None
					);
				}
			} else {
				List<ChainTypeId> fragments =
					MetaplayClient.PlayerModel.GameConfig.IslandBuildingFragments[applicationInfo.ActiveIsland.Value];
				ChainTypeId type = fragments[0];
				uiRootController.ShowUI<BuildingInstructionsPopup, BuildingPopupPayload>(new BuildingPopupPayload(type, ItemModel.Info.Type), CancellationToken.None);
			}
		}

		private void Update() {
			if (!IsActive || !RewardsReady) {
				return;
			}

			notificationBubbleTimer += Time.deltaTime;

			if (notificationBubbleTimer > 3.0f) {
				ShowNotificationBubbleAsync().Forget();
				notificationBubbleTimer = 0;
			}
		}

		private async UniTask ShowNotificationBubbleAsync() {
			NotificationBubble.SetActive(true);
			await NotificationBubble.transform.DOPunchPosition(new Vector3(0, 50, 0), 0.2f);
			await UniTask.Delay(2000);
			if (NotificationBubble != null) {
				NotificationBubble.SetActive(false);
			}
		}
	}
}
