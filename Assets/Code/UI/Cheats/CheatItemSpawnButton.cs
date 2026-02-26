using System;
using Code.UI.Application;
using Code.UI.AssetManagement;
using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Cheats {
	public class CheatItemSpawnButton : ButtonHelper {
		[Inject] private AddressableManager addressableManager;
		[Inject] private ApplicationInfo applicationInfo;

		private LevelId<ChainTypeId> chainId;

		public void Setup(LevelId<ChainTypeId> item) {
			chainId = item;

			GetComponentInChildren<TMP_Text>().text = $"{item.Type} {item.Level}";

			var image = GetComponent<Image>();

			try {
				image.sprite = addressableManager.GetItemIcon(chainId);
			} catch (Exception) {
				image.sprite = null;
			}
		}

		protected override void OnClick() {
			var activeIsland = applicationInfo.ActiveIsland.Value;

			if (activeIsland == null) {
				return;
			}

			MetaplayClient.PlayerContext.ExecuteAction(
				new CheatAddMergeItem(activeIsland, chainId.Type, chainId.Level)
			);
		}
	}
}
