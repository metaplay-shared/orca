using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerClaimMinedItems)]
	public class PlayerClaimMinedItems : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerClaimMinedItems() { }

		public PlayerClaimMinedItems(IslandTypeId islandId, int x, int y) {
			IslandId = islandId;
			X = x;
			Y = y;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(IslandId)) {
				return ActionResult.InvalidParam;
			}

			if (player.Islands[IslandId].State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			IslandModel island = player.Islands[IslandId];
			MergeBoardModel mergeBoard = island.MergeBoard;
			if (X < 0 || X >= mergeBoard.Info.BoardWidth) {
				return ActionResult.InvalidCoordinates;
			}

			if (Y < 0 || Y >= mergeBoard.Info.BoardHeight) {
				return ActionResult.InvalidCoordinates;
			}

			ItemModel item = mergeBoard[X, Y].Item;
			if (item == null) {
				return ActionResult.InvalidCoordinates;
			}

			if (item.Mine == null) {
				return ActionResult.InvalidCoordinates;
			}

			if (item.State != ItemState.Free) {
				return ActionResult.InvalidState;
			}

			if (item.Mine.State != MineState.ItemsComplete) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				int count = 0;
				while (item.Mine.Queue.Count > 0) {
					Coordinates coordinates = mergeBoard.FindClosestFreeTile(X, Y);
					if (coordinates == null) {
						break;
					}

					LevelId<ChainTypeId> newItemId = item.Mine.Queue[0];
					ChainTypeId newType = player.MapRewardToRealType(IslandId, newItemId.Type);
					if (newType == ChainTypeId.None) {
						item.Mine.Queue.RemoveAt(0);
						continue;
					}
					ItemModel newItem = new ItemModel(
						newType,
						newItemId.Level,
						player.GameConfig,
						player.CurrentTime,
						true
					);
					newItem.AcknowledgeBuilding();
					item.Mine.Queue.RemoveAt(0);
					mergeBoard.CreateItem(coordinates.X, coordinates.Y, newItem);
					player.ClientListener.OnItemCreatedOnBoard(IslandId, newItem, X, Y, coordinates.X, coordinates.Y, true);
					if (newItem.CanBeDiscovered) {
						player.HandleItemDiscovery(newItem);
					}
					count++;
				}

				if (item.Mine.Queue.Count == 0) {
					item.Mine.UpdateState(player.GameConfig);
					if (item.Mine.State == MineState.Destroyed) {
						mergeBoard.RemoveItem(X, Y, player.ClientListener);
						mergeBoard.CalculateItemStates(player.HandleItemDiscovery, player.ClientListener);
						if (item.Mine.Info.DestructionItem.Type != ChainTypeId.None) {
							ItemModel destructionItem = new ItemModel(
								player.MapRewardToRealType(IslandId, item.Mine.Info.DestructionItem.Type),
								item.Mine.Info.DestructionItem.Level,
								player.GameConfig,
								player.CurrentTime,
								true
							);
							destructionItem.AcknowledgeBuilding();
							mergeBoard.CreateItem(X, Y, destructionItem);
							player.ClientListener.OnItemCreatedOnBoard(
								mergeBoard.Info.Type,
								destructionItem,
								X,
								Y,
								X,
								Y,
								true
							);
							if (destructionItem.CanBeDiscovered) {
								player.HandleItemDiscovery(destructionItem);
							}
						}
					} else if (item.Mine.Info.AutoRepair) {
						int builderId = player.Builders.AssignTaskToConsumable(IslandId, player.CurrentTime, item.Mine.RepairTime);
						item.Mine.StartRepairing(builderId);
						player.ClientListener.OnMergeItemStateChanged(IslandId, item);
					}
				}
				island.RunIslandTaskTriggers(player.ExecuteTrigger);
				player.ClientListener.OnMergeItemStateChanged(IslandId, item);
				player.EventStream.Event(
					new PlayerMinedItemsClaimed(IslandId, item.Info.Type, item.Mine.Info.Level, count, item.Mine.Queue.Count)
				);
			}

			return ActionResult.Success;
		}
	}
}
