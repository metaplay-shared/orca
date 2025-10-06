using Game.Logic;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;
using System.Collections.Generic;
using System.Linq;
using Code.UI.Events;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.ItemDiscovery {
	public class DiscoveryContentPresenter : MonoBehaviour {
		[SerializeField] private RectTransform TabButtons;
		[SerializeField] private ToggleGroup TabsToggleGroup;
		[SerializeField] private RectTransform ScrollContent;
		[SerializeField] private ItemDiscoveryTabButton TemplateTabButton;
		[SerializeField] private ItemDiscoveryCategoryContent TemplateDiscoveryCategoryContent;
		[SerializeField] private FloatingClaimedReward FloatingClaimedRewardPrefab;
		[SerializeField] private RectTransform FloatingClaimedRewardContainer;
		[SerializeField] private ScrollRect ScrollRect;

		[Inject] private DiContainer container;

		private readonly Dictionary<CategoryId, ItemDiscoveryTabButton> tabButtons = new();

		protected void Awake() {
			FloatingClaimedRewardPrefab.gameObject.SetActive(false);
			SpawnTabButtons();
		}

		protected void OnEnable() {
			ScrollRect.verticalNormalizedPosition = 1f;
			OrderedSet<CategoryId> somethingToClaim =
				MetaplayClient.PlayerModel.Merge.ItemDiscovery.SomethingToClaimCategories(
					MetaplayClient.PlayerModel.GameConfig
				);
			CategoryId categoryToSelect = somethingToClaim.FirstOrDefault() ??
				MetaplayClient.PlayerModel.GameConfig.Global.DefaultDiscoveryCategory;
			TabsToggleGroup.SetAllTogglesOff();
			SelectCategory(categoryToSelect);
		}

		private void SelectCategory(CategoryId categoryToSelect) {
			ItemDiscoveryTabButton categoryToggle = tabButtons.GetValueOrDefault(categoryToSelect);
			if (categoryToggle == null) {
				Debug.LogWarning($"Couldn't find categoryToggle with id: '{categoryToSelect.Value}'");
				return;
			}

			categoryToggle.Toggle.isOn = true;
		}

		private void SpawnTabButtons() {
			bool originalTemplateActiveState = TemplateDiscoveryCategoryContent.gameObject.activeSelf;
			TemplateDiscoveryCategoryContent.gameObject.SetActive(false);

			foreach (CategoryId category in MetaplayClient.PlayerModel.GameConfig.Global.DiscoveryCategories) {
				DiContainer subContainer = container.CreateSubContainer();
				subContainer.Bind<CategoryId>().FromInstance(category).AsSingle();
				subContainer.Bind<FloatingClaimedReward>().FromInstance(FloatingClaimedRewardPrefab).AsSingle();
				subContainer.Bind<RectTransform>().WithId("FloatingClaimedRewardContainer")
					.FromInstance(FloatingClaimedRewardContainer).AsSingle();

				ItemDiscoveryTabButton tabToggle =
					subContainer.InstantiatePrefabForComponent<ItemDiscoveryTabButton>(TemplateTabButton, TabButtons);
				tabButtons.Add(category, tabToggle);

				ItemDiscoveryCategoryContent categoryContent =
					subContainer.InstantiatePrefabForComponent<ItemDiscoveryCategoryContent>(
						TemplateDiscoveryCategoryContent,
						ScrollContent
					);
				categoryContent.gameObject.SetActive(false);

				tabToggle.Toggle.onValueChanged.AddListener(isOn => SetCategoryActive(categoryContent, isOn));
				tabToggle.Toggle.group = TabsToggleGroup;
			}

			TemplateDiscoveryCategoryContent.gameObject.SetActive(originalTemplateActiveState);
		}

		private void SetCategoryActive(ItemDiscoveryCategoryContent categoryContent, bool isOn) {
			if (categoryContent.gameObject.activeInHierarchy) {
				ScrollRect.DOVerticalNormalizedPos(1f, 0.2f);
			} else {
				ScrollRect.verticalNormalizedPosition = 1f;
			}
			categoryContent.gameObject.SetActive(isOn);
		}
	}
}
