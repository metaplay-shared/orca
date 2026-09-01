using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerCreateMergeItem)]
	public class PlayerCreateMergeItem : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerCreateMergeItem() { }

		public PlayerCreateMergeItem(IslandTypeId islandId, int x, int y) {
			IslandId = islandId;
			X = x;
			Y = y;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(IslandId)) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[IslandId];
			if (island.State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			MergeBoardModel mergeBoard = island.MergeBoard;

			if (X < 0 || X >= mergeBoard.Info.BoardWidth) {
				return ActionResult.InvalidCoordinates;
			}
			if (Y < 0 || Y >= mergeBoard.Info.BoardHeight) {
				return ActionResult.InvalidCoordinates;
			}

			ItemModel originItem = mergeBoard[X, Y].Item;
			if (originItem == null) {
				return ActionResult.InvalidCoordinates;
			}

			if (!originItem.CanCreate || originItem.LockedState != ItemLockedState.Open) {
				return ActionResult.InvalidState;
			}

			if (!originItem.HasItems) {
				return ActionResult.NoItemsLeft;
			}

			if (player.Merge.Energy.ProducedAtUpdate < originItem.Creator.Info.EnergyUsage) {
				return ActionResult.NotEnoughResources;
			}

			Coordinates coordinates = mergeBoard.FindClosestFreeTile(X, Y);
			if (coordinates == null) {
				// No free tiles, nothing to do. This is not really an error.
				return ActionResult.InvalidState;
			}

			if (commit) {
				if (originItem.Creator.Info.EnergyUsage > 0) {
					player.ConsumeResources(
						CurrencyTypeId.Energy,
						originItem.Creator.Info.EnergyUsage,
						ResourceModificationContext.Empty
					);
				}

				if (originItem.Creator.ItemCount == 0) {
					originItem.Converter.UseCurrentItem(originItem.Creator);
				}

				ItemModel item = originItem.Creator.CreateItem(
					IslandId,
					player.Random,
					player.GameConfig,
					player.CurrentTime,
					player.MapRewardToRealType
				);
				mergeBoard.CreateItem(coordinates.X, coordinates.Y, item);
				player.ClientListener.OnItemCreatedOnBoard(
					mergeBoard.Info.Type,
					item,
					X,
					Y,
					coordinates.X,
					coordinates.Y,
					true
				);
				if (item.CanBeDiscovered) {
					player.HandleItemDiscovery(item);
					island.RunIslandTaskTriggers(player.ExecuteTrigger);

					if (island.IsCompleteBuildingFragment(player.GameConfig, item)) {
						island.UpdateBuildingStartState(player.GameConfig, player.HandleBuildingState);
					}
				}

				if (originItem.Creator.ItemCount == 0) {
					if (originItem.Creator.WavesLeft > 0) {
						originItem.Creator.UseWave();
					} else if (originItem.Creator.Info.Disposable) {
						mergeBoard.RemoveItem(X, Y, player.ClientListener);
						mergeBoard.CalculateItemStates(player.HandleItemDiscovery, player.ClientListener);
						island.UpdateBuildingState(player.GameConfig, player.HandleBuildingState, player.ClientListener);
						if (originItem.Creator.Info.DestructionItem.Type != ChainTypeId.None) {
							ItemModel destructionItem = new ItemModel(
								player.MapRewardToRealType(IslandId, originItem.Creator.Info.DestructionItem.Type),
								originItem.Creator.Info.DestructionItem.Level,
								player.GameConfig,
								player.CurrentTime,
								true
							);
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
								island.RunIslandTaskTriggers(player.ExecuteTrigger);
							}
						}
					}
				}
			}

			return ActionResult.Success;
		}
	}
}
