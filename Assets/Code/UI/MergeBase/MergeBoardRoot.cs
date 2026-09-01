using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Code.UI.Application.Signals;
using Code.UI.Island.Signals;
using Code.UI.Merge;
using Code.UI.Merge.AddOns;
using Code.UI.MergeBase.Signals;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Code.UI.MergeBase {
	public class MergeBoardRoot : MonoBehaviour {
		public const float TILE_WIDTH = 120;
		public const float TILE_HEIGHT = 120;

		[SerializeField] private RectTransform TileContainer;
		[SerializeField] private RectTransform ItemContainer;
		[SerializeField] private RectTransform ItemOverlayLayer;
		[SerializeField] private MergeItem MergeItemPrefab;

		[SerializeField] private MergeTile PrefabCell;

		[Inject] private DiContainer container;
		[Inject] private SignalBus signalBus;

		public List<MergeTile> Tiles { get; } = new();
		private readonly List<MergeItem> items = new();

		private MergeItem selectedItem;

		internal IBoardStateAdapter BoardState;
		private MergeTile shipTile;
		private DateTime lastMergeHintTime = DateTime.Now;

		private float MergeHintDelay = 5;


		public async UniTask RebuildView(List<MergeTileModel> boardLayout, BoardStateAdapter boardState) {
			BoardState = boardState;
			CreateBoard(boardLayout);

			foreach (var mergeAddOn in GetComponents<MergeBoardAddOn>()) {
				container.Inject(mergeAddOn);
				mergeAddOn.Setup(BoardState.Identifier);
			}

			await RestoreStateAsync();

			foreach (MergeTileModel tile in boardLayout) {
				if (tile.Special == (int) TileType.Ship) {
					shipTile = TileAt(tile.X, tile.Y);
				}
			}
		}

		private void OnEnable() {
			signalBus.Subscribe<ItemCreatedSignal>(OnItemCreated);
			signalBus.Subscribe<ItemMovedSignal>(OnItemMoved);
			signalBus.Subscribe<ItemRemovedSignal>(OnItemRemoved);
			signalBus.Subscribe<ItemMergedSignal>(OnItemMerged);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<ItemCreatedSignal>(OnItemCreated);
			signalBus.Unsubscribe<ItemMovedSignal>(OnItemMoved);
			signalBus.Unsubscribe<ItemRemovedSignal>(OnItemRemoved);
			signalBus.Unsubscribe<ItemMergedSignal>(OnItemMerged);
			Clear();
		}

		private void OnItemRemoved(ItemRemovedSignal signal) {
			RemoveAt(TileAt(signal.X, signal.Y));
		}

		private void OnItemMoved(ItemMovedSignal signal) {
			if (signal.BoardIdentifier != BoardState.Identifier) {
				return;
			}

			MergeItem item = items.Find(i => i.Adapter.Id == signal.Item);
			if (item == null) {
				Debug.LogWarning("The moved item does not exist");
				return;
			}
			MergeTile to = TileAt(signal.ToX, signal.ToY);

			// TODO: User a proper cancellation token.
			item.MoveToAsync(default, to, false).Forget();
		}

		private void OnItemCreated(ItemCreatedSignal signal) {
			// Note, OnItemCreated may be called right after OnEnable which would cause NRE since the board has not been
			// initialized yet.
			if (BoardState == null || signal.IslandTypeId.Value != BoardState.Identifier) {
				return;
			}

			CreateItem(signal.Item, signal.FromX, signal.FromY, default).Forget();
		}

		private void OnItemMerged(ItemMergedSignal signal) {
			if (signal.Island.Value != BoardState.Identifier) {
				return;
			}

			Select(ItemAt(signal.Item.X, signal.Item.Y), false, true);
		}

		private void RemoveAt(MergeTile at) {
			if (at == null || !at.HoldsItem) {
				return;
			}

			at.Item.DestroySelfAsync(default).Forget();
			items.Remove(items.FirstOrDefault(i => i.LastKnownX == at.X && i.LastKnownY == at.Y));
		}

		public MergeTile TileAt(int x, int y) {
			return Tiles.FirstOrDefault(c => c.X == x && c.Y == y);
		}

		internal void Select(MergeItem item, bool canOpen, bool created = false) {
			if (selectedItem != null && selectedItem != item) {
				selectedItem.OnDeselected();
			}

			if (item == null) {
				signalBus.Fire(new ItemSelectedSignal(null));
			} else {
				item.OnSelected(created);
				if (canOpen && (selectedItem == item || item.Adapter.QuickOpen)) {
					item.Open();
				}
				signalBus.Fire(new ItemSelectedSignal(item.Adapter));
			}

			selectedItem = item;
		}

		private void CreateBoard(List<MergeTileModel> inputTiles) {
			foreach (MergeTileModel tile in inputTiles) {
				CreateTile(tile);
			}
		}

		private void CreateTile(MergeTileModel tileModel) {
			MergeTile tileInstance = container.InstantiatePrefab(PrefabCell, TileContainer).GetComponent<MergeTile>();
			tileInstance.name = $@"{tileModel.X}, {tileModel.Y}";
			RectTransform tileRectTransform = tileInstance.GetComponent<RectTransform>();

			tileRectTransform.anchoredPosition = CalculateTilePosition(tileModel.X, tileModel.Y);
			tileRectTransform.sizeDelta = new Vector2(TILE_WIDTH, TILE_HEIGHT);

			tileInstance.X = tileModel.X;
			tileInstance.Y = tileModel.Y;

			bool isAlternativeTile = (tileModel.X + tileModel.Y) % 2 == 0;

			tileInstance.Setup(
				this,
				isAlternativeTile,
				isInvisible: tileModel.Special == (int) TileType.ItemHolder || tileModel.Special == (int) TileType.Ship
			);
			RegisterCell(tileInstance);
		}

		private Vector2 CalculateTilePosition(int x, int y) {
			return new Vector2(
				x * TILE_WIDTH + TILE_WIDTH / 2,
				y * TILE_HEIGHT + TILE_HEIGHT / 2
			);
		}

		private async UniTask RestoreStateAsync() {
			List<UniTask> tasks = new();

			foreach (IMergeItemModelAdapter item in BoardState.MergeItems) {
				tasks.Add(
					CreateItem(
						item,
						item.X,
						item.Y,
						default,
						item.X * 50
					)
				);
			}

			await UniTask.WhenAll(tasks);
		}

		private async UniTask CreateItem(
			IMergeItemModelAdapter adapter,
			int fromX,
			int fromY,
			CancellationToken ct,
			int delayMillis = 0
		) {
			if (delayMillis > 0) {
				// Wave-like spawn effect
				await UniTask.Delay(delayMillis, cancellationToken: ct);
			}

			MergeTile tile = TileAt(fromX, fromY);
			MergeItem mergeItem = container.InstantiatePrefabForComponent<MergeItem>(MergeItemPrefab, ItemContainer);

			if (tile != null) {
				mergeItem.transform.position = tile.transform.position;
			} else {
				if (shipTile == null) {
					// We should not get here unless there is a configuration mistake on the island map.
					MergeTile targetTile = TileAt(adapter.X, adapter.Y);
					MergeTile targetTileNeighbor = TileAt(adapter.X + 1, adapter.Y);
					// Cheats use 0,0 as source coordinate and it may be in the sea
					if (targetTileNeighbor != null) {
						Vector3 targetTilePosition = targetTile.transform.position;
						Vector3 diff = targetTileNeighbor.transform.position - targetTilePosition;
						mergeItem.transform.position = targetTilePosition - diff;
					}
				} else {
					mergeItem.transform.position = shipTile.transform.position;
				}
			}

			mergeItem.name = $"{adapter.Type}{adapter.Level}";
			RegisterItem(mergeItem);

			mergeItem.Setup(adapter, ItemOverlayLayer, ct);
			await mergeItem.MoveToAsync(ct, TileAt(adapter.X, adapter.Y), true);
		}

		private void RegisterCell(MergeTile tile) {
			Tiles.Add(tile);
		}

		private void RegisterItem(MergeItem item) {
			items.Add(item);
		}

		public void Clear() {
			foreach (Transform child in ItemContainer) {
				Destroy(child.gameObject);
			}

			items.Clear();

			ClearTiles();
			ClearOverlay();
		}

		private void ClearOverlay() {
			foreach (Transform child in ItemOverlayLayer.transform) {
				Destroy(child.gameObject);
			}
		}

		private void ClearTiles() {
			foreach (var tile in Tiles) {
				Destroy(tile.gameObject);
			}

			Tiles.Clear();
		}

		public MergeItem ItemAt(int x, int y) {
			return items.FirstOrDefault(
				i =>
					x >= i.LastKnownX &&
					x < i.LastKnownX + i.Adapter.Width &&
					y >= i.LastKnownY &&
					y < i.LastKnownY + i.Adapter.Height
			);
		}

		private void Update() {
			if ((DateTime.Now - lastMergeHintTime).TotalSeconds > MergeHintDelay) {
				lastMergeHintTime = DateTime.Now;
				DoMergeHint().Forget();
			}
		}

		private async UniTask DoMergeHint() {
			var validItems = items
				// Discard hidden items
				.Where(i => i.Adapter.State is ItemState.Free or ItemState.FreeForMerge)
				.Where(i => i.Adapter.BuildState == ItemBuildState.Complete)
				.Where(i => !i.Adapter.IsBubble)

				// Discard items that are used in task
				.Where(i => !i.Adapter.IsUsedInTask)

				// Discard items that are under lock area
				.Where(i => !i.Adapter.UnderLockArea)

				// Discard max level items
				.Where(i => !i.Adapter.IsMaxLevel)

				// Group by type+level
				.GroupBy(i => i.Adapter.Type + i.Adapter.Level.ToString())
				.Select(g => g.ToList())
				.Where(g => g.Count > 1)
				.Where(g => g.Any(i => i.Adapter.State == ItemState.Free))

				// Randomize order
				.OrderBy(_ => Random.Range(0.0f, 1.0f))
				.FirstOrDefault();

			if (validItems == null) {
				return;
			}

			var first = validItems
				.OrderBy(i => i.Adapter.State != ItemState.Free)
				.First();

			validItems.Remove(first);

			var last = validItems
				.OrderBy(i => i.Adapter.State == ItemState.Free)
				.First();

			var direction = (last.Handle.transform.position - first.Handle.transform.position)
				.normalized;

			try {
				// TODO: Use a proper cancellation token
				await first.NudgeAsync(default, direction, strength: 20f);
				await last.BounceAsync(default, strength: 0.5f);

				first.Handle.transform.localScale = Vector3.one;
				last.Handle.transform.localScale = Vector3.one;
			} catch {
				// Items were destroyed mid-animation.
			}
		}

		private static List<MergeTileModel> BuildLayout(IslandModel island) {
			List<MergeTileModel> layout = new List<MergeTileModel>();

			for (int y = 0; y < island.MergeBoard.Info.BoardHeight; y++) {
				for (int x = 0; x < island.MergeBoard.Info.BoardWidth; x++) {
					var pattern = island.Info.BoardPattern[x, y];
					if (pattern != TileType.Sea) {
						layout.Add(
							new MergeTileModel() {
								X = x,
								Y = y,
								Special = (int)pattern
							}
						);
					}
				}
			}

			return layout;
		}

		public async UniTask SetupBoardAsync(IslandTypeId islandType) {
			var islandModel = MetaplayClient.PlayerModel.Islands[islandType];
			var layout = BuildLayout(islandModel);

			BoardStateAdapter boardState = new(islandModel.MergeBoard.Items, islandType, container);

			await RebuildView(layout, boardState);
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerEnterIsland(islandType));
			foreach (ItemModel item in islandModel.MergeBoard.Items) {
				if (item.BuildState == ItemBuildState.PendingComplete) {
					MetaplayClient.PlayerContext.ExecuteAction(
						new PlayerAcknowledgeBuilding(islandType, item.X, item.Y)
					);
				}
			}
			signalBus.Fire(new EnteredIslandSignal());
		}
	}
}
