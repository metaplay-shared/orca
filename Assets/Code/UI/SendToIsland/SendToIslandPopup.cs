using Code.UI.Application;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.SendToIsland {
	public class SendToIslandPopupHandle : UIHandleBase {
		public ItemModel ItemModel { get; }

		public SendToIslandPopupHandle(ItemModel itemModel) {
			ItemModel = itemModel;
		}
	}

	public class SendToIslandPopup : UIRootBase<SendToIslandPopupHandle> {
		[SerializeField] private RectTransform ItemsParent;
		[SerializeField] private Image ItemImage;
		[SerializeField] private SendToIslandItem SendToIslandItemTemplate;
		[SerializeField] private Button CloseButton;

		[Inject] private AddressableManager addressableManager;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private DiContainer diContainer;

		private readonly List<SendToIslandItem> items = new();

		private void Clear() {
			items.Clear();
			foreach (Transform child in ItemsParent) {
				Destroy(child.gameObject);
			}
		}

		protected override void Init() {
			Clear();
			Sprite sprite = addressableManager.GetItemIcon(UIHandle.ItemModel.Info);
			ItemImage.sprite = sprite;
			foreach (IslandModel island in MetaplayClient.PlayerModel.Islands.Values) {
				if (island.State == IslandState.Open &&
					island.Info.Type != IslandTypeId.EnergyIsland &&
					island.Info.Type != applicationInfo.ActiveIsland.Value
				) {
					SendToIslandItem sendToIslandItem =
						diContainer.InstantiatePrefabForComponent<SendToIslandItem>(SendToIslandItemTemplate, ItemsParent);
					sendToIslandItem.Setup(island.Info.Type, UIHandle.ItemModel);
					items.Add(sendToIslandItem);
				}
			}
		}

		protected override UniTask Idle(CancellationToken ct) {
			return UniTask.WhenAny(
				UniTask.WhenAny(items.Select(i => i.OnSendToIslandAsync(ct)).ToArray()),
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}
	}
}
