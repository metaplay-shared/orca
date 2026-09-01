using System.Collections.Generic;
using System.Threading;
using Code.UI.Application;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using Orca.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Rewarding {
	public class RewardPopupPayload : UIHandleBase {
		public readonly RewardModel Reward;

		public RewardPopupPayload(RewardModel reward) {
			Reward = reward;
		}
	}

	public class RewardPopup : UIRootBase<RewardPopupPayload> {
		[SerializeField] private TMP_Text Title;
		[SerializeField] private Button ClaimRewardButton;
		[SerializeField] private RectTransform Rewards;
		[SerializeField] private Image HeroImage;
		[SerializeField] private RectTransform RewardResourceContainer;
		[SerializeField] private RectTransform RewardItemContainer;
		[SerializeField] private RewardItem TemplateRewardItem;

		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private DiContainer container;
		[Inject] private AddressableManager addressableManager;

		protected override void Init() {
			HeroImage.gameObject.SetActive(false);
			foreach (Transform child in Rewards) {
				child.gameObject.SetActive(false);
			}

			RewardModel reward = UIHandle.Reward;
			RewardMetadata rewardMetadata = reward.Metadata;
			RewardType rewardType = rewardMetadata.Type;

			SetupIcons(reward.Items, reward.Resources);
			SetTitle(
				rewardType switch {
					RewardType.PlayerLevel      => Localizer.Localize("Info.LevelUp"),
					RewardType.HeroUnlock       => Localizer.Localize("Info.HeroUnlocked"),
					RewardType.BuildingFragment => Localizer.Localize("Info.BuildingFragmentDone"),
					RewardType.ActivityEventLevel => Localizer.Localize(
						"Info.ActivityEventLevelUp",
						rewardMetadata.Level,
						GetActivityEventLevelCount(rewardMetadata) - 1
					),
					RewardType.IslandTask => Localizer.Localize("Info.IslandTaskDone"),
					// TODO: Add human readable name in the config for VIP pass and use that in the title.
					RewardType.VipPassDaily => Localizer.Localize(
						"Info.VipPassDailyReward",
						rewardMetadata.VipPass.Value
					),
					_ => Localizer.Localize("Info.GoodJob")
				}
			);

			if (rewardType == RewardType.HeroUnlock) {
				HeroImage.gameObject.SetActive(true);
				HeroImage.sprite = addressableManager.Get<Sprite>($"Heroes/{rewardMetadata.Hero}.png");
			}

			static int GetActivityEventLevelCount(RewardMetadata rewardMetadata) {
				Option<List<ActivityEventLevelInfo>> levels =
					MetaplayClient.PlayerModel.GameConfig.ActivityEventLevelsByEvent.GetValueOrDefault(
						rewardMetadata.Event
					).ToOption();
				return levels.Map(l => l.Count).GetOrElse(rewardMetadata.Level);
			}
		}

		protected override async UniTask Idle(CancellationToken ct) {
			await ClaimRewardButton.OnClickAsync(ct);
		}

		protected override void HandleAndroidBackButtonPressed() {
			// No backwards functionality
		}

		private void SetTitle(string localizedTitle) {
			Debug.Log($"Set title to {localizedTitle}");
			Title.text = localizedTitle;
		}

		private void SetupIcons(List<ItemCountInfo> rewardItems, List<ResourceInfo> rewardResources) {
			foreach (ResourceInfo rewardResource in rewardResources) {
				SpawnResourceIcon(rewardResource);
			}

			RewardResourceContainer.gameObject.SetActive(rewardResources.Count > 0);

			foreach (ItemCountInfo rewardItem in rewardItems) {
				SpawnItemIcon(rewardItem);
			}

			RewardItemContainer.gameObject.SetActive(rewardItems.Count > 0);
		}

		private void SpawnItemIcon(ItemCountInfo rewardItem) {
			RewardItem item = container.InstantiatePrefab(TemplateRewardItem).GetComponent<RewardItem>();
			item.transform.SetParent(RewardItemContainer, false);
			ChainTypeId realType = MetaplayClient.PlayerModel.MapRewardToRealTypeUI(
				applicationInfo.ActiveIsland.Value,
				rewardItem.Type
			);
			item.Setup(realType, rewardItem.Level, rewardItem.Count);
		}

		private void SpawnResourceIcon(ResourceInfo resource) {
			RewardItem item = container.InstantiatePrefab(TemplateRewardItem).GetComponent<RewardItem>();
			item.transform.SetParent(RewardResourceContainer, false);
			item.Setup(resource.Type, resource.Amount);
		}
	}
}
