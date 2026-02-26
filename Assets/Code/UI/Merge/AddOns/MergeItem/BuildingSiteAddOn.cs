using Code.UI.Application;
using Code.UI.AssetManagement;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class BuildingSiteAddOn : ItemAddOn {
		[SerializeField] private GameObject AddOnRoot;
		[SerializeField] private RectTransform[] Fragments;
		[Inject] private SignalBus signalBus;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private AddressableManager addressableManager;

		protected override void Setup() {
			if (ItemModel.Info.Building) {
				AddOnRoot.SetActive(true);
			} else {
				AddOnRoot.SetActive(false);
			}
			SetupFragments();
		}

		private void OnEnable() {
			signalBus.Subscribe<BuildingChangedSignal>(SetupFragments);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<BuildingChangedSignal>(SetupFragments);
		}

		private void SetupFragments() {
			if (ItemModel.Info.Level > 1) {
				return;
			}

			var allFragments = MetaplayClient.PlayerModel.GameConfig.IslandBuildingFragments[applicationInfo.ActiveIsland.Value];
			foreach (int slot in MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value].CompletedBuildingSlots.Keys) {
				if (slot < Fragments.Length) {
					RectTransform fragmentParent = Fragments[slot];
					SpawnBuilding(allFragments[slot], fragmentParent);
				}
			}
		}

		private void SpawnBuilding(ChainTypeId type, RectTransform parent) {
			foreach (Transform child in parent) {
				Destroy(child.gameObject);
			}

			int maxLevel = MetaplayClient.PlayerModel.GameConfig.ChainMaxLevels.GetMaxLevel(type);
			GameObject buildingGo = new GameObject("Building");
			Image buildingImage = buildingGo.AddComponent<Image>();
			Sprite buildingGraphic = addressableManager.GetItemIcon(type, maxLevel);
			buildingImage.sprite = buildingGraphic;

			RectTransform buildingTransform = buildingGo.GetComponent<RectTransform>();
			buildingTransform.sizeDelta = gameObject.GetComponent<RectTransform>().sizeDelta / 2;
			buildingTransform.SetParent(parent, false);
			buildingTransform.anchoredPosition = Vector3.zero;
		}
	}
}
