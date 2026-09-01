using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class MergeBoardModel {
		[MetaMember(1)] public IslandInfo Info { get; private set; }
		[MetaMember(2)] public List<ItemModel> Items { get; private set; }
		[MetaMember(3)] public List<ItemModel> ItemHolder { get; private set; }
		[MetaMember(4)] public LockAreaModel LockArea { get; private set; }
		[MetaMember(5)] public bool BuildingRevealed { get; private set; }
		[IgnoreDataMember] private MergeTileModel[] Tiles { get; set; }
		[IgnoreDataMember] private List<Coordinates> ItemHolderTiles { get; set; }
		// This is kept as a member variable to reduce unnecessary garbage generation when task items are calculated.
		[IgnoreDataMember] private MetaDictionary<LevelId<ChainTypeId>, int> usedTaskItemCounts = new();

		public MergeBoardModel() {}

		public MergeBoardModel(
			SharedGameConfig gameConfig,
			IslandTypeId islandId,
			MetaTime currentTime,
			Action<ItemModel> discoveryHandler
		) {
			Info = gameConfig.Islands[islandId];
			Items = new List<ItemModel>();
			ItemHolder = new List<ItemModel>();
			Init();

			LockArea = new LockAreaModel(Info.LockAreaPattern);
			foreach (InitialItemInfo itemInfo in gameConfig.InitialItems.Values) {
				if (itemInfo.IslandId == Info.Type && itemInfo.Type != ChainTypeId.None) {
					if (this[itemInfo.X, itemInfo.Y].Type != TileType.Ground) {
						throw new Exception($"Invalid initial item config on island {islandId}, coordinate ({itemInfo.X}, {itemInfo.Y}) is not on ground");
					}
					ItemModel item = new ItemModel(itemInfo.Type, itemInfo.Level, gameConfig, currentTime, itemInfo.InitAsBuilt) {
						State = itemInfo.Free ? ItemState.Free : ItemState.Hidden,
						SkipFreeForMerge = itemInfo.SkipFreeForMerge
					};
					CreateItem(itemInfo.X, itemInfo.Y, item);
					if (item.CanBeDiscovered && LockArea.IsFree(itemInfo.X, itemInfo.Y)) {
						discoveryHandler.Invoke(item);
					}
				}
			}

			CalculateItemStates(discoveryHandler, EmptyPlayerModelClientListener.Instance);
		}

		public void InitGame(Action<ItemModel> discoveryHandler) {
			foreach (var item in Items) {
				if (item.CanBeDiscovered && LockArea.IsFree(item.X, item.Y)) {
					discoveryHandler.Invoke(item);
				}
			}
		}

		public void Init() {
			Tiles = new MergeTileModel[Info.BoardWidth * Info.BoardHeight];
			ItemHolderTiles = new List<Coordinates>();
			int dockTileXSum = 0;
			for (int i = 0; i < Tiles.Length; i++) {
				MergeTileModel tile = new MergeTileModel(Info.BoardPattern.TileAt(i));
				Tiles[i] = tile;
				if (tile.Type == TileType.ItemHolder) {
					int x = i % Info.BoardWidth;
					int y = i / Info.BoardWidth;
					ItemHolderTiles.Add(new Coordinates(x, y));
					dockTileXSum += x;
				}
			}

			if (ItemHolderTiles.Count > 0) {
				int dockCenterX = dockTileXSum / ItemHolderTiles.Count;
				ItemHolderTiles.Sort(
					(c1, c2) => dockCenterX < Info.BoardWidth / 2 ? c1.X.CompareTo(c2.X) : c2.X.CompareTo(c1.X)
				);
			}

			foreach (ItemModel item in Items) {
				SetItem(item, item.X, item.Y);
			}
		}

		public void CalculateItemStates(Action<ItemModel> discoveryHandler, IPlayerModelClientListener listener) {
			bool newPassNeeded;
			do {
				newPassNeeded = false;
				for (int i = 0; i < Info.BoardWidth; i++) {
					for (int j = 0; j < Info.BoardHeight; j++) {
						MergeTileModel tile = this[i, j];
						if (tile.HasItem) {
							ItemModel item = tile.Item;
							// Skip the calculation for the tiles that are not the main tile (real location) of a larger
							// merge item.
							if (item.X != i || item.Y != j) {
								continue;
							}

							if (!LockArea.IsFree(item.X, item.Y)) {
								continue;
							}

							ItemState originalState = item.State;
							for (int k = 0; k < item.Info.Width; k++) {
								item.State = GetUpdatedState(i + k, j + item.Info.Height, item.State, false);
								item.State = GetUpdatedState(i + k, j - 1, item.State, false);
							}
							for (int k = 0; k < item.Info.Height; k++) {
								item.State = GetUpdatedState(i + item.Info.Width, j + k, item.State, false);
								item.State = GetUpdatedState(i - 1, j + k, item.State, false);
							}

							item.State = GetUpdatedState(i + item.Info.Width, j + item.Info.Height, item.State, true);
							item.State = GetUpdatedState(i + item.Info.Width, j - 1, item.State, true);
							item.State = GetUpdatedState(i - 1, j + item.Info.Height, item.State, true);
							item.State = GetUpdatedState(i - 1, j - 1, item.State, true);

							if (item.State == ItemState.FreeForMerge && item.SkipFreeForMerge) {
								item.State = ItemState.Free;
								newPassNeeded = true;
							}

							if (originalState != item.State) {
								if (item.State == ItemState.FreeForMerge &&
									(item.Creator != null && !item.Creator.Info.Disposable || item.Mine != null)) {
									item.State = ItemState.Free;
									newPassNeeded = true;
								}
								listener.OnMergeItemStateChanged(Info.Type, item);
							}

							if (!BuildingRevealed && item.Info.Building && (item.State == ItemState.Free || item.State == ItemState.FreeForMerge)) {
								BuildingRevealed = true;
							}

							if (item.CanBeDiscovered) {
								discoveryHandler.Invoke(item);
							}
						}
					}
				}
			} while (newPassNeeded);
		}

		/// <summary>
		/// <c>GetUpdatedState</c> calculates the new <c>ItemState</c> for an item based on the state of its
		/// neighboring tile at (x,y).
		/// </summary>
		/// <param name="x">X coordinate of a tile that might change its neighbor's state</param>
		/// <param name="y">Y coordinate of a tile that might change its neighbor's state</param>
		/// <param name="originalState">state of the item before the calculation</param>
		/// <param name="diagonal">whether (x,y) is a diagonal neighbor of the item whose state to update</param>
		/// <returns>the updated state of an item</returns>
		private ItemState GetUpdatedState(int x, int y, ItemState originalState, bool diagonal) {
			// Free and FreeForMerge are "end states" that are not affected by the states of neighboring items or tiles.
			if (originalState == ItemState.Free || originalState == ItemState.FreeForMerge) {
				return originalState;
			}
			MergeTileModel tile = this[x, y];
			// "Out-of-board" tiles don't affect item states.
			if (tile == null) {
				return originalState;
			}
			// Only unlocked ground tiles might affect item states.
			if (Info.BoardPattern[x, y] != TileType.Ground || !LockArea.IsFree(x, y)) {
				return originalState;
			}

			ItemModel item = tile.Item;
			// An empty tile and a free item are equivalent: only they can change the state of a neighbouring item.
			if (item == null || item.State == ItemState.Free) {
				if (diagonal) {
					// A diagonal empty tile or free item will convert Hidden into PartiallyVisible
					return ItemState.PartiallyVisible;
				} else {
					// A radial empty tile or free item will convert Hidden or PartiallyVisible into FreeForMerge
					return ItemState.FreeForMerge;
				}
			}

			return originalState;
		}

		/// <summary>
		/// CanMoveFrom checks whether there's an item at (x,y) that can be moved elsewhere.
		/// </summary>
		public bool CanMoveFrom(int x, int y) {
			if (x < 0 || x >= Info.BoardWidth || y < 0 || y >= Info.BoardHeight) {
				return false;
			}

			ItemModel item = this[x, y].Item;
			return item != null && item.CanMove && LockArea.IsFree(x, y);
		}

		public enum MoveResultType {
			Invalid,
			Merge,
			Move,
			Building,
			BuilderBoost
		}

		public MoveResultType MoveResult(SharedGameConfig gameConfig, ItemModel item, int x, int y) {
			if (x < 0 || x >= Info.BoardWidth || y < 0 || y >= Info.BoardHeight) {
				return MoveResultType.Invalid;
			}

			if (item.Info.Width == 1 && item.Info.Height == 1) {
				MergeTileModel targetTile = this[x, y];
				if (targetTile.Type != TileType.Ground) {
					return MoveResultType.Invalid;
				}

				if (!LockArea.IsFree(x, y)) {
					return MoveResultType.Invalid;
				}

				if (targetTile.HasItem) {
					ItemModel targetItem = targetTile.Item;
					if (targetItem.State == ItemState.PartiallyVisible || targetItem.State == ItemState.Hidden) {
						return MoveResultType.Invalid;
					}

					if (item.CanApplyBuilderTo(targetItem)) {
						return MoveResultType.BuilderBoost;
					}
					bool canMerge = item.CanMergeWith(targetItem, gameConfig);
					if (!canMerge && targetItem.State == ItemState.FreeForMerge) {
						return MoveResultType.Invalid;
					}

					if (targetItem.Bubble) {
						return MoveResultType.Invalid;
					}

					if (canMerge) {
						return MoveResultType.Merge;
					}

					if (this[item.X, item.Y].Type == TileType.ItemHolder && !HasFreeSlots()) {
						return MoveResultType.Invalid;
					}

					return targetItem.Info.Movable || targetItem.Info.Building
						? MoveResultType.Move
						: MoveResultType.Invalid;
				}

				return MoveResultType.Move;
			}

			for (int i = 0; i < item.Info.Width; i++) {
				for (int j = 0; j < item.Info.Height; j++) {
					MergeTileModel targetTile = this[x + i, y + j];
					if (targetTile == null || targetTile.Type != TileType.Ground) {
						return MoveResultType.Invalid;
					}
					if (!LockArea.IsFree(x + i, y + j)) {
						return MoveResultType.Invalid;
					}
					if (targetTile.HasItem && targetTile.Item != item) {
						return MoveResultType.Invalid;
					}
				}
			}
			return MoveResultType.Move;
		}

		private bool HasFreeSlots() {
			for (int i = 0; i < Info.BoardWidth; i++) {
				for (int j = 0; j < Info.BoardHeight; j++) {
					MergeTileModel targetTile = this[i, j];
					if (targetTile != null && targetTile.IsFree && LockArea.IsFree(i, j)) {
						return true;
					}
				}
			}

			return false;
		}

		public void Update(
			RandomPCG random,
			MetaTime currentTime,
			SharedGameConfig gameConfig,
			IPlayerModelClientListener listener,
			Func<IslandTypeId, ChainTypeId, ChainTypeId> typeMapper,
			Action<ItemModel> discoveryHandler
		) {
			for (int i = 0; i < Info.BoardWidth; i++) {
				for (int j = 0; j < Info.BoardHeight; j++) {
					MergeTileModel tile = this[i, j];
					if (tile.Item == null) {
						continue;
					}

					ItemModel item = tile.Item;
					if (item.Bubble && item.OpenAt <= currentTime) {
						RemoveItem(i, j, listener);
						if (gameConfig.Global.MergeBubbleSpawn.Type != ChainTypeId.None) {
							ItemModel bubbleItem = new ItemModel(
								gameConfig.Global.MergeBubbleSpawn.Type,
								gameConfig.Global.MergeBubbleSpawn.Level,
								gameConfig,
								currentTime,
								true
							);
							CreateItem(i, j, bubbleItem);
							listener.OnItemCreatedOnBoard(Info.Type, bubbleItem, i, j, i, j, true);
						}
					} else if (item.Creator?.Info.AutoSpawn == true && item.LockedState == ItemLockedState.Open) {
						while (true) {
							Coordinates coordinates = FindFreeNeighbor(i, j);
							if (coordinates == null) {
								item.Update(Info.Type, currentTime, listener);
								break;
							}

							MetaTime nextTime =
								item.Creator.Producer.LastUpdated +
								item.Creator.TimeToFill(item.Creator.Producer.LastUpdated);
							if (nextTime > currentTime) {
								nextTime = currentTime;
							}
							item.Update(Info.Type, nextTime, listener);

							if (item.Creator.ItemCount > 0) {
								ItemModel spawnedItem = item.Creator.CreateItem(
									Info.Type,
									random,
									gameConfig,
									nextTime,
									typeMapper
								);
								CreateItem(coordinates.X, coordinates.Y, spawnedItem);
								listener.OnItemCreatedOnBoard(
									Info.Type,
									spawnedItem,
									i,
									j,
									coordinates.X,
									coordinates.Y,
									true
								);
								discoveryHandler.Invoke(spawnedItem);
								if (item.Creator.ItemCount == 0) {
									if (item.Creator.WavesLeft > 0) {
										item.Creator.UseWave();
									} else if (item.Creator.Info.Disposable) {
										RemoveItem(item.X, item.Y, listener);
										CalculateItemStates(discoveryHandler, listener);
										if (item.Creator.Info.DestructionItem.Type != ChainTypeId.None) {
											ItemModel destructionItem = new ItemModel(
												typeMapper.Invoke(Info.Type, item.Creator.Info.DestructionItem.Type),
												item.Creator.Info.DestructionItem.Level,
												gameConfig,
												currentTime,
												true
											);
											CreateItem(item.X, item.Y, destructionItem);
											listener.OnItemCreatedOnBoard(
												Info.Type,
												destructionItem,
												item.X,
												item.Y,
												item.X,
												item.Y,
												true
											);
											if (destructionItem.CanBeDiscovered) {
												discoveryHandler.Invoke(destructionItem);
												// TODO: We're not running island task triggers here. Not really needed
												// but this logic should be refactored into a common method.
											}
										}
									}
								}
							} else {
								item.Update(Info.Type, currentTime, listener);
								break;
							}

							if (!gameConfig.Global.FastAutoSpawn) {
								break;
							}
						}
					} else {
						tile.Item.Update(Info.Type, currentTime, listener);
					}
				}
			}
		}

		/// <summary>
		/// <c>FinishBuilders</c> finishes the build process for all items that are supposed to be finished. The item
		/// is considered to be finished if the state is <c>ItemBuildState.Building</c> and the builder ID assigned to
		/// the item is no longer in the list of occupied builders.
		/// </summary>
		/// <param name="gameConfig">The game configuration</param>
		/// <param name="random">A random number generator to be used in item creation</param>
		/// <param name="occupiedBuilders">A list of builder IDs that are currently occupied</param>
		/// <param name="listener">Client listener for any updates</param>
		/// <param name="discoveryHandler">Handler method for new item discovery</param>
		public void FinishBuilders(SharedGameConfig gameConfig, RandomPCG random, OrderedSet<int> occupiedBuilders, IPlayerModelClientListener listener, Action<ItemModel> discoveryHandler) {
			foreach (ItemModel item in Items) {
				if (item.BuildState == ItemBuildState.Building && !occupiedBuilders.Contains(item.BuilderId)) {
					item.FinishBuilding();
					listener.OnMergeItemStateChanged(Info.Type, item);
					listener.OnBuilderFinished(item);
				}

				if (item.Mine != null) {
					if ((item.Mine.State == MineState.Mining || item.Mine.State == MineState.Repairing) &&
						!occupiedBuilders.Contains(item.Mine.BuilderId)) {
						item.Mine.UpdateState(gameConfig);
						listener.OnMergeItemStateChanged(Info.Type, item);
						listener.OnBuilderFinished(item);
					}
				}
			}
		}

		public void HandleAutoMine(SharedGameConfig gameConfig, RandomPCG random, IPlayerModelClientListener listener) {
			foreach (ItemModel item in Items) {
				if (item.Mine != null) {
					if (item.Mine.Info.AutoMine && item.Mine.State == MineState.Idle) {
						item.Mine.CreateItems(random);
						item.Mine.StartMining(0);
						item.Mine.UpdateState(gameConfig);
						listener.OnMergeItemStateChanged(Info.Type, item);
					}
				}
			}
		}

		public void CreateItem(int x, int y, ItemModel item) {
			Items.Add(item);
			SetItem(item, x, y);
			item.SetLocation(x, y);
		}

		public void MoveItem(ItemModel item, int x, int y, IPlayerModelClientListener listener) {
			int originalX = item.X;
			int originalY = item.Y;
			ClearItem(item, originalX, originalY);
			SetItem(item, x, y);
			item.SetLocation(x, y);

			if (this[originalX, originalY].Type == TileType.ItemHolder) {
				AdjustItemHolder(listener);
			}
		}

		public void RemoveItem(int x, int y, IPlayerModelClientListener listener) {
			ItemModel item = this[x, y].Item;
			Items.Remove(item);
			ClearItem(item, x, y);
			listener.OnItemRemovedFromBoard(Info.Type, item, x, y);

			if (this[x, y].Type == TileType.ItemHolder) {
				AdjustItemHolder(listener);
			}
		}

		public bool HasItemsOnDock() {
			foreach (Coordinates coordinates in ItemHolderTiles) {
				if (this[coordinates.X, coordinates.Y].HasItem) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// An item holder of a board can be thought of consisting of the row of "dock tiles" and an (invisible) queue
		/// of items that cannot fit onto the dock. <c>AdjustItemHolder</c> packs items on the dock to the far right
		/// and pops items from the queue onto the dock until either the queue is empty or the dock is full.
		/// </summary>
		/// <param name="listener">client listener to notify about changes</param>
		public void AdjustItemHolder(IPlayerModelClientListener listener) {
			for (int i = ItemHolderTiles.Count - 1; i >= 0; i--) {
				Coordinates coordinates = ItemHolderTiles[i];
				MergeTileModel tile = this[coordinates.X, coordinates.Y];
				if (!tile.HasItem) {
					for (int j = i - 1; j >= 0; j--) {
						Coordinates source = ItemHolderTiles[j];
						MergeTileModel sourceTile = this[source.X, source.Y];
						if (sourceTile.HasItem) {
							ItemModel item = sourceTile.Item;
							MoveItem(item, coordinates.X, coordinates.Y, listener);
							listener.OnItemMovedOnBoard(
								Info.Type,
								item,
								source.X,
								source.Y,
								coordinates.X,
								coordinates.Y
							);
							break;
						}
					}

					if (!tile.HasItem) {
						if (ItemHolder.Count > 0) {
							ItemModel poppedItem = ItemHolder[0];
							ItemHolder.RemoveAt(0);
							listener.OnItemHolderModified(Info.Type);
							CreateItem(coordinates.X, coordinates.Y, poppedItem);
							listener.OnItemCreatedOnBoard(
								Info.Type,
								poppedItem,
								StaticConfig.ItemHolderX,
								StaticConfig.ItemHolderY,
								coordinates.X,
								coordinates.Y,
								true
							);
						}
					}
				}
			}
		}

		[IgnoreDataMember] // \note [jarkko-metaplay] Hack to work around JSON.net defaults getting confused. Real fix: custom serializer
		public MergeTileModel this[int x, int y] {
			get {
				if (x < 0 || x >= Info.BoardWidth) {
					return null;
				}
				if (y < 0 || y >= Info.BoardHeight) {
					return null;
				}
				return Tiles[x + y * Info.BoardWidth];
			}
		}

		public int GetWaterTileCode(int x, int y) {
			if (!IsWater(x, y)) {
				return -1;
			}

			int bit0 = IsWater(x, y - 1) ? 0 : 1;
			int bit1 = IsWater(x + 1, y) ? 0 : 1;
			int bit2 = IsWater(x, y + 1) ? 0 : 1;
			int bit3 = IsWater(x - 1, y) ? 0 : 1;

			int bit4 = !IsWater(x - 1, y - 1) && bit3 == 0 && bit0 == 0 ? 1 : 0;
			int bit5 = !IsWater(x + 1, y - 1) && bit0 == 0 && bit1 == 0 ? 1 : 0;
			int bit6 = !IsWater(x + 1, y + 1) && bit1 == 0 && bit2 == 0 ? 1 : 0;
			int bit7 = !IsWater(x - 1, y + 1) && bit2 == 0 && bit3 == 0 ? 1 : 0;

			return bit7 << 7 | bit6 << 6 | bit5 << 5 | bit4 << 4 | bit3 << 3 | bit2 << 2 | bit1 << 1 | bit0;
		}

		private bool IsWater(int x, int y) {
			if (x < 0 || x >= Info.BoardWidth) {
				return true;
			}
			if (y < 0 || y >= Info.BoardHeight) {
				return true;
			}

			TileType type = this[x, y].Type;
			return type == TileType.Sea || type == TileType.ItemHolder || type == TileType.Ship;
		}

		public AreaState AreaLockState(char areaIndex) => LockArea.AreaLockState(areaIndex);

		/// <summary>
		/// FindCoordinates finds the coordinates of an item on board using reference equality.
		/// </summary>
		/// <param name="item">The item model to find</param>
		/// <returns>The coordinates of the item or null if not found on board</returns>
		public Coordinates FindCoordinates(ItemModel item) {
			for (int i = 0; i < Info.BoardWidth; i++) {
				for (int j = 0; j < Info.BoardHeight; j++) {
					if (this[i, j].Item == item) {
						return new Coordinates(i, j);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// FindFreeNeighbor finds a free, unlocked tile next to (x,y). The neighboring tiles are searched
		/// in the order illustrated below (bottom to top, left to right):
		/// <code>
		/// 247
		/// 1*6
		/// 035
		/// </code>
		/// </summary>
		/// <param name="x">X coordinate of the point whose free neighbor to find</param>
		/// <param name="y">Y coordinate of the point whose free neighbor to find</param>
		/// <returns>A free neighbor of (x,y) or null if no free neighbors exist</returns>
		public Coordinates FindFreeNeighbor(int x, int y) {
			int minX = Math.Max(0, x - 1);
			int maxX = Math.Min(Info.BoardWidth - 1, x + 1);
			int minY = Math.Max(0, y - 1);
			int maxY = Math.Min(Info.BoardHeight - 1, y + 1);
			for (int i = minX; i <= maxX; i++) {
				for (int j = minY; j <= maxY; j++) {
					if ((i != x || j != y) && this[i, j].IsFree && LockArea.IsFree(i, j)) {
						return new Coordinates(i, j);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// FindClosestFreeTile finds the closest free, unlocked tile near (x,y) by searching incrementally growing
		/// "square neighbourhoods" of (x,y). The example below illustrates the order of tiles searched
		/// when finding the closest free tile of (1,1). The search order is marked with hexadecimal digits (0-F)
		/// with 0 being the starting point of the search itself.
		/// <code>
		/// 4|######
		/// 3|CDEF##
		/// 2|678B##
		/// 1|405A##
		/// 0|1239##
		/// --------
		///   012345
		/// </code>
		/// </summary>
		/// <param name="x">X coordinate of the point whose closest free tile to find</param>
		/// <param name="y">Y coordinate of the point whose closest free tile to find</param>
		/// <returns>The closest free tile of (x,y) or null if no free tiles exist</returns>
		public Coordinates FindClosestFreeTile(int x, int y) {
			int minX = x;
			int minY = y;
			int maxX = x;
			int maxY = y;

			do {
				int cappedMinX = Math.Max(0, minX);
				int cappedMaxX = Math.Min(Info.BoardWidth - 1, maxX);
				int cappedMinY = Math.Max(0, minY);
				int cappedMaxY = Math.Min(Info.BoardHeight - 1, maxY);
				for (int i = cappedMinY; i <= cappedMaxY; i++) {
					// Skip the middle area as it was already handled on the previous cycle.
					int increment = i == cappedMinY || i == cappedMaxY ? 1 : cappedMaxX - cappedMinX;
					// The very first cycle would be with 0 increment if we didn't do this.
					increment = Math.Max(1, increment);
					for (int j = cappedMinX; j <= cappedMaxX; j += increment) {
						if (this[j, i].IsFree && LockArea.IsFree(j, i)) {
							return new Coordinates(j, i);
						}
					}
				}

				minX -= 1;
				maxX += 1;
				minY -= 1;
				maxY += 1;
			} while (minX >= 0 || minY >= 0 || maxX <= Info.BoardWidth - 1 || maxY <= Info.BoardHeight - 1);

			return null;
		}

		/// <summary>
		/// <c>TotalItemCount</c> returns the number of items on the board
		/// (including hidden items and items under locked areas). <seealso cref="ItemCount"/>
		/// </summary>
		public int TotalItemCount => Items.Count;

		public bool HasItemsForTask(IslandTaskInfo info) {
			usedTaskItemCounts.Clear();
			foreach (ItemCountInfo items in info.Items) {
				int used = usedTaskItemCounts.GetValueOrDefault(items.ChainId);
				if (ItemCount(
					    items.Type,
					    items.Level,
					    includeLockedAreas: false,
					    item => item.CanMove) < items.Count + used) {
					return false;
				}

				usedTaskItemCounts[items.ChainId] = used + items.Count;
			}

			return true;
		}

		/// <summary>
		/// <para> ItemCount returns the number of items on board matching the criteria. Each criteria can be ignored
		/// by using its default value (for example <c>null</c> for <paramref name="type"/>).</para>
		/// <para>To get the total number of items on the board you should use <see cref="TotalItemCount"/></para>
		/// </summary>
		/// <param name="type">type of items to count or <c>null</c> for any type</param>
		/// <param name="level">level of items to count or <c>0</c> for any level</param>
		/// <param name="includeLockedAreas">whether to count items under locked areas</param>
		/// <param name="test">function to test which items to include in the count or <c>null</c> for no test</param>
		/// <returns>The number of items matching the criteria</returns>
		public int ItemCount(
			ChainTypeId type = null,
			int level = 0,
			bool includeLockedAreas = true,
			Func<ItemModel, bool> test = null) {
			int total = 0;
			foreach (var item in Items) {
				if ((type == null || type == item.Info.Type) &&
				    (level <= 0 || level == item.Info.Level) &&
				    (includeLockedAreas || LockArea.IsFree(item.X, item.Y)) &&
				    (test == null || test(item))) {
					total++;
				}
			}

			return total;
		}

		/// <summary>
		/// <c>RemoveItems</c> removes from unlocked areas on the board at most <paramref name="count"/> items
		/// that match the given criteria.
		/// </summary>
		/// <param name="type">type of items to remove</param>
		/// <param name="level">level of items to remove. Negative value inverts the condition i.e. <c>-5</c> removes
		/// items with <c>level != 5</c>.</param>
		/// <param name="count">number of items to remove. The actual number of items removed can be smaller if
		/// the board does not contain enough items that match the criteria.</param>
		/// <param name="force">consider also unmovable items applicable for removal</param>
		/// <param name="listener">client listener to notify of removed items</param>
		/// <returns>number of removed items</returns>
		public int RemoveItems(ChainTypeId type, int level, int count, bool force, IPlayerModelClientListener listener) {
			if (count <= 0) {
				return 0;
			}
			int total = 0;
			for (int i = 0; i < Info.BoardWidth; i++) {
				for (int j = 0; j < Info.BoardHeight; j++) {
					MergeTileModel tile = this[i, j];
					if (tile.HasItem && (tile.Item.CanMove || force) && tile.Item.Info.Type == type
						&& (level < 0 && tile.Item.Info.Level != -level || tile.Item.Info.Level == level)) {
						if (LockArea.IsFree(i, j)) {
							RemoveItem(i, j, listener);
							total++;
							if (total == count) {
								return total;
							}
						}
					}
				}
			}

			return total;
		}

		public bool BuildingsPendingComplete() {
			return ItemCount(
					type: null,
					level: 0,
					includeLockedAreas: false,
					item => item.BuildState == ItemBuildState.PendingComplete
				) > 0;
		}

		public bool MinesWithItemsComplete() {
			return ItemCount(
				type: null,
				level: 0,
				includeLockedAreas: false,
				item => item.Mine != null && item.Mine.State == MineState.ItemsComplete
			) > 0;
		}

		public void ManageLockAreas(
			int playerLevel,
            MetaDictionary<HeroTypeId, HeroModel> heroes,
			SharedGameConfig gameConfig,
			IPlayerModelClientListener listener
		) {
			List<LockAreaInfo> lockAreas = gameConfig.IslandLockAreas[Info.Type];
			foreach (LockAreaInfo lockArea in lockAreas) {
				if (lockArea.PlayerLevel <= playerLevel &&
					(string.IsNullOrEmpty(lockArea.Dependency) || LockArea.Areas.GetValueOrDefault(lockArea.Dependency[0]) == AreaState.Open) &&
					(lockArea.Hero == HeroTypeId.None ||
						heroes.ContainsKey(lockArea.Hero) && heroes[lockArea.Hero].Level.Level >= lockArea.HeroLevel)) {
					if (LockArea.Areas.GetValueOrDefault(lockArea.AreaIndex) == AreaState.Locked) {
						LockArea.UnlockArea(lockArea.AreaIndex);
						listener.OnLockAreaUnlocked(Info.Type, lockArea.AreaIndex);
					}
				} else {
					LockArea.LockArea(lockArea.AreaIndex);
				}
			}
		}

		private void SetItem(ItemModel item, int x, int y) {
			for (int i = 0; i < item.Info.Width; i++) {
				for (int j = 0; j < item.Info.Height; j++) {
					this[x + i, y + j].Item = item;
				}
			}
		}

		private void ClearItem(ItemModel item, int x, int y) {
			for (int i = 0; i < item.Info.Width; i++) {
				for (int j = 0; j < item.Info.Height; j++) {
					if (this[x + i, y + j].Item == item) {
						this[x + i, y + j].Item = null;
					}
				}
			}
		}

		public ItemModel FindItem(Func<ItemModel, bool> test) {
			foreach (ItemModel item in Items) {
				if (test(item)) {
					return item;
				}
			}

			return null;
		}

		public void MarkBuildingComplete(SharedGameConfig gameConfig, MetaTime currentTime, IPlayerModelClientListener listener) {
			ItemModel buildingItem = FindItem(i => i.Info.Building);
			if (buildingItem != null) {
				RemoveItem(buildingItem.X, buildingItem.Y, listener);
				ItemModel newBuildingItem = new ItemModel(buildingItem.Info.Type, 2, gameConfig, currentTime, true);
				CreateItem(buildingItem.X, buildingItem.Y, newBuildingItem);
				listener.OnItemCreatedOnBoard(
					Info.Type,
					newBuildingItem,
					buildingItem.X,
					buildingItem.Y,
					buildingItem.X,
					buildingItem.Y,
					false
				);
			}
		}

		public void ReplaceItems(SharedGameConfig gameConfig, MetaTime currentTime, ReplacementContextId context, IPlayerModelClientListener listener) {
			for (int i = 0; i < Items.Count; i++) {
				ItemModel item = Items[i];
				ReplacementId id = new ReplacementId(context, item.Info.Type, item.Info.Level);
				if (gameConfig.ItemReplacements.ContainsKey(id)) {
					ItemReplacementInfo replacement = gameConfig.ItemReplacements[id];
					RemoveItem(item.X, item.Y, listener);
					if (replacement.ReplacementType != ChainTypeId.None) {
						ItemModel newItem = new ItemModel(
							replacement.ReplacementType,
							replacement.ReplacementLevel,
							gameConfig,
							currentTime,
							true
						);
						CreateItem(item.X, item.Y, newItem);
						listener.OnItemCreatedOnBoard(Info.Type, newItem, item.X, item.Y, item.X, item.Y, false);
					}

					i--;
				} else {
					if (item.Creator != null) {
						ReplaceItemsInQueue(gameConfig, context, item.Creator.ItemQueue);
					}

					if (item.Mine != null) {
						ReplaceItemsInQueue(gameConfig, context, item.Mine.Queue);
					}
				}
			}

			for (int i = 0; i < ItemHolder.Count; i++) {
				ItemModel item = ItemHolder[i];
				ReplacementId id = new ReplacementId(context, item.Info.Type, item.Info.Level);
				if (gameConfig.ItemReplacements.ContainsKey(id)) {
					ItemReplacementInfo replacement = gameConfig.ItemReplacements[id];
					if (replacement.ReplacementType == ChainTypeId.None) {
						ItemHolder.RemoveAt(i);
						i--;
					} else {
						ItemHolder[i] = new ItemModel(
							replacement.ReplacementType,
							replacement.ReplacementLevel,
							gameConfig,
							currentTime,
							true
						);
					}
				} else {
					if (item.Creator != null) {
						ReplaceItemsInQueue(gameConfig, context, item.Creator.ItemQueue);
					}

					if (item.Mine != null) {
						ReplaceItemsInQueue(gameConfig, context, item.Mine.Queue);
					}
				}
			}
		}

		private void ReplaceItemsInQueue(SharedGameConfig gameConfig, ReplacementContextId context, List<LevelId<ChainTypeId>> queue) {
			for (int i = 0; i < queue.Count; i++) {
				LevelId<ChainTypeId> itemId = queue[i];
				ReplacementId id = new ReplacementId(context, itemId.Type, itemId.Level);
				if (gameConfig.ItemReplacements.ContainsKey(id)) {
					ItemReplacementInfo replacement = gameConfig.ItemReplacements[id];
					if (replacement.ReplacementType == ChainTypeId.None) {
						queue.RemoveAt(i);
						i--;
					} else {
						queue[i] = new LevelId<ChainTypeId>(
							replacement.ReplacementType,
							replacement.ReplacementLevel
						);
					}
				}
			}
		}
	}
}
