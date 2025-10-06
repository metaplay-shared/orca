using System.Collections.Generic;
using System.Threading;
using Code.UI.Application;
using Code.UI.Core;
using Code.UI.InfoMessage.Signals;
using Code.UI.MergeBase;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.ItemRemoval {
	public class ItemRemovalButton : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler {

		[SerializeField] private RectTransform RemovalLayer;
		[SerializeField] private RectTransform RemovalTool;
		[SerializeField] private RectTransform RemovalToolIcon;
		[Inject] private MergeBoardRoot mergeBoard;
		[Inject] private SignalBus signalBus;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IUIRootController uiRootController;

		public void OnBeginDrag(PointerEventData eventData) {
			RemovalLayer.gameObject.SetActive(true);
			RemovalTool.position = RemovalToolIcon.position;

			foreach (MergeTile tile in mergeBoard.Tiles) {
				if (tile.HoldsItem && tile.Item.Adapter.CanRemove) {
					CopyItem(tile);
				}
			}
		}

		public void OnEndDrag(PointerEventData eventData) {
			MergeTile tile = FindDragEndTile(eventData);
			if (tile != null && tile.HoldsItem && tile.Item.Adapter.CanRemove) {
				RemoveItem(tile, CancellationToken.None).Forget();
			}

			RemovalLayer.gameObject.SetActive(false);
			ClearOverlay();
		}

		private async UniTask RemoveItem(MergeTile tile, CancellationToken ct) {
			ItemModel itemModel = (ItemModel) tile.Item.Adapter.Id;
			ConfirmationPopupHandle handle = uiRootController.ShowUI<ConfirmationPopup, ConfirmationPopupHandle>(
				new ConfirmationPopupHandle(
					Localizer.Localize("Info.RemoveItemTitle"),
					Localizer.Localize("Info.RemoveItemContent", itemModel.Info.Localize()),
					ConfirmationPopupHandle.ConfirmationPopupType.YesNo
				),
				ct
			);

			ConfirmationPopupResult result = await handle.OnCompleteWithResult;

			if (result.Response == ConfirmationPopupResponse.Yes) {
				MetaplayClient.PlayerContext.ExecuteAction(
					new PlayerSellMergeItem(applicationInfo.ActiveIsland.Value, tile.X, tile.Y)
				);
			}
		}

		private MergeTile FindDragEndTile(PointerEventData eventData) {
			List<RaycastResult> raycastResults = new();
			EventSystem.current.RaycastAll(eventData, raycastResults);
			foreach (var result in raycastResults) {
				MergeTile target = result.gameObject.GetComponent<MergeTile>();
				if (target != null) {
					return target;
				}
			}

			return null;
		}

		public void OnDrag(PointerEventData eventData) {
			RemovalTool.position = eventData.pointerCurrentRaycast.worldPosition;
		}

		public void OnPointerClick(PointerEventData eventData) {
			signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.RemovalToolInstruction")));
		}

		private void CopyItem(MergeTile tile) {
			MergeItem item = tile.Item;
			GameObject itemGameObject = item.Handle;
			Sprite sprite = item.ItemSprite;

			GameObject newItem = new GameObject();
			Image flyingImage = newItem.AddComponent<Image>();
			flyingImage.raycastTarget = false;
			flyingImage.sprite = sprite;
			RectTransform newItemRt = newItem.GetComponent<RectTransform>();
			newItemRt.SetParent(RemovalLayer, false);
			newItemRt.sizeDelta = itemGameObject.GetComponent<RectTransform>().sizeDelta;
			newItemRt.position = itemGameObject.transform.position;
			newItemRt.SetAsFirstSibling();
		}

		private void ClearOverlay() {
			foreach (Transform child in RemovalLayer) {
				if (child != RemovalTool) {
					Destroy(child.gameObject);
				}
			}
		}
	}
}
