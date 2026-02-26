using Code.UI.Application;
using Code.UI.AssetManagement;
using Code.UI.ItemHolder.Signals;
using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.ItemHolder {
	public class ItemHolder : ButtonHelper {
		[SerializeField] private Image TopItemImage;

		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private AddressableManager addressableManager;

		protected override void OnEnable() {
			base.OnEnable();
			ShowTopItemAsync();
			signalBus.Subscribe<ItemHolderModifiedSignal>(OnItemHolderModified);
		}

		protected override void OnDisable() {
			base.OnDisable();

			signalBus.Unsubscribe<ItemHolderModifiedSignal>(OnItemHolderModified);
		}

		private void OnItemHolderModified() {
			ShowTopItemAsync();
		}

		protected override void OnClick() {
			PopItem();
		}

		private void ShowTopItemAsync() {
			if (applicationInfo.ActiveIsland.Value == null) {
				TopItemImage.gameObject.SetActive(false);
				return;
			}

			var info = MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value].MergeBoard.ItemHolder.Count > 0
				? MetaplayClient.PlayerModel.Islands[applicationInfo.ActiveIsland.Value].MergeBoard.ItemHolder[0].Info
				: default;

			if (info == null) {
				TopItemImage.gameObject.SetActive(false);
				return;
			}

			TopItemImage.gameObject.SetActive(true);
			TopItemImage.sprite = addressableManager.GetItemIcon(info);
		}

		private void PopItem() {
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerPopMergeItem(applicationInfo.ActiveIsland.Value));
		}
	}
}
