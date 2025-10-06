// Comment this to disable mass-spawning

//#define MERGE_HOLD_TO_SPAWN

using System;
using System.Collections.Generic;
using Code.UI.Application;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.MergeBase {
	public class MergeTileModel {
		public int X;
		public int Y;
		public long Special;
	}

	public class MergeTile : MonoBehaviour,
			IBeginDragHandler,
			IDragHandler,
			IEndDragHandler,
			IPointerClickHandler
			#if MERGE_HOLD_TO_SPAWN
			,
			IPointerDownHandler,
			IPointerUpHandler
		#endif
	{
		public int X { get; set; }
		public int Y { get; set; }
		public GameObject Handle => gameObject;

		public bool HoldsItem => Item != null;
		private bool CanDrag => HoldsItem && Item.Interactable;
		public MergeItem Item => mergeBoard.ItemAt(X, Y);

		[SerializeField] private Image TileImage;
		[SerializeField] private Sprite AlternativeTileGraphic;

		[Inject] private Canvas canvas;
		[Inject] private IFrameRateController frameRateController;

		private MergeBoardRoot mergeBoard;
		private bool isDragging;

		#if MERGE_HOLD_TO_SPAWN
		private bool isPointerDown;
		private float holdTimer;
		#endif

		private MergeItem draggedItem;
		private RectTransform draggedItemRt;
		private MergeTile originTile;
		private MergeTile targetTile;
		private IDisposable fpsHandle;

		public void Setup(MergeBoardRoot mergeBoardRoot, bool alternativeGraphic, bool isInvisible = false) {
			mergeBoard = mergeBoardRoot;

			if (isInvisible) {
				TileImage.color = Color.clear;
			}
			else if (alternativeGraphic) {
				TileImage.sprite = AlternativeTileGraphic;
			}
		}

		public void OnBeginDrag(PointerEventData eventData) {
			if (!CanDrag) {
				return;
			}

			if (!mergeBoard.BoardState.CanMoveFrom(X, Y)) {
				return;
			}

			eventData.eligibleForClick = false;

			draggedItem = Item;
			mergeBoard.Select(draggedItem, false);
			draggedItem.OnBeginDrag();
			draggedItemRt = draggedItem.Handle.GetComponent<RectTransform>();
			originTile = mergeBoard.TileAt(Item.Adapter.X, Item.Adapter.Y);
			isDragging = true;

			draggedItem.StopAnimations();

			fpsHandle = frameRateController.RequestHighFPS();
		}

		public void OnDrag(PointerEventData eventData) {
			if (!isDragging) {
				return;
			}

			eventData.position -= (Vector2) canvas.worldCamera.ScreenToWorldPoint(new Vector2(
				(draggedItem.Adapter.Width - 1.0f) / 2 * MergeBoardRoot.TILE_WIDTH,
				(draggedItem.Adapter.Height - 1.0f) / 2 * MergeBoardRoot.TILE_HEIGHT
			));
			draggedItemRt.position = eventData.pointerCurrentRaycast.worldPosition;

			var raycastResults = new List<RaycastResult>();
			DrawDebugMarker(eventData.position);
			EventSystem.current.RaycastAll(eventData, raycastResults);

			targetTile = null;

			foreach (var result in raycastResults) {
				if (result.gameObject.TryGetComponent<Blackout>(out var _)) {
					break;
				}

				var resultTile = result.gameObject.GetComponent<MergeTile>();

				// This is an intended reference comparison
				#pragma warning disable 252,253
				if (resultTile == null ||
					resultTile == originTile) {
					continue;
				}
				#pragma warning restore 252,253

				targetTile = resultTile;
			}

			bool hoverOnMergeTarget = targetTile != null &&
				mergeBoard.BoardState.CanMergeTo(originTile.X, originTile.Y, targetTile.X, targetTile.Y);
			draggedItem.OnHoverMergeTarget(
				hoverOnMergeTarget,
				targetTile != null
					? mergeBoard.ItemAt(targetTile.X, targetTile.Y)
					: null
			);
		}

		public void OnEndDrag(PointerEventData eventData) {
			if (fpsHandle != null) {
				fpsHandle.Dispose();
				fpsHandle = null;
			}

			if (!isDragging) {
				return;
			}

			isDragging = false;

			draggedItem.OnEndDrag();

			if (targetTile != null) {
				if (mergeBoard.BoardState.CanMoveTo(originTile.X, originTile.Y, targetTile.X, targetTile.Y)) {
					mergeBoard.BoardState.MoveItem(originTile.X, originTile.Y, targetTile.X, targetTile.Y);
				} else {
					// TODO: Use a proper CancellationToken
					draggedItem.MoveToAsync(default, originTile, false).Forget();
				}
			} else {
				// TODO: Use a proper CancellationToken
				draggedItem.MoveToAsync(default, originTile, false).Forget();
			}

			draggedItem = null;
			originTile = null;
		}

		public void OnPointerClick(PointerEventData eventData) {
			DoSelect();
		}

		private void DoSelect() {
			if (Item != null && !Item.Interactable) {
				return;
			}
			mergeBoard.Select(HoldsItem ? Item : null, true);
		}

		private void Update() {
			#if MERGE_HOLD_TO_SPAWN
			if (isPointerDown & !isDragging) {
				holdTimer -= Time.deltaTime;

				if (holdTimer <= 0.0f) {
					holdTimer = 0.2f;
					DoSelect();
				}
			}
			#endif
		}

		#if MERGE_HOLD_TO_SPAWN
		public void OnPointerDown(PointerEventData eventData) {
			isPointerDown = true;
			holdTimer = 0.5f;
		}

		public void OnPointerUp(PointerEventData eventData) {
			isPointerDown = false;
		}
		#endif

		private void DrawDebugMarker(Vector3 position) {
			float size = 4;
			var topLeft = position - new Vector3(-size, size, 0);
			var topRight = position - new Vector3(size, size, 0);
			var bottomLeft = position - new Vector3(-size, -size, 0);
			var bottomRight = position - new Vector3(size, -size, 0);

			Debug.DrawLine(topLeft, topRight, Color.red, 0.1f, false);
			Debug.DrawLine(topRight, bottomRight, Color.red, 0.1f, false);
			Debug.DrawLine(bottomRight, bottomLeft, Color.red, 0.1f, false);
			Debug.DrawLine(bottomLeft, topLeft, Color.red, 0.1f, false);
		}
	}
}
