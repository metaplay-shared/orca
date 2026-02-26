using Game.Logic.LiveOpsEvents;
using Metaplay.Core;
using Metaplay.Core.LiveOpsEvent;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerMoveItemOnBoard)]
	public class PlayerMoveItemOnBoard : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int FromX { get; private set; }
		public int FromY { get; private set; }
		public int ToX { get; private set; }
		public int ToY { get; private set; }

		public PlayerMoveItemOnBoard() { }

		public PlayerMoveItemOnBoard(IslandTypeId islandId, int fromX, int fromY, int toX, int toY) {
			IslandId = islandId;
			FromX = fromX;
			FromY = fromY;
			ToX = toX;
			ToY = toY;
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
			if (!mergeBoard.CanMoveFrom(FromX, FromY)) {
				return ActionResult.InvalidCoordinates;
			}

			if (FromX == ToX && FromY == ToY) {
				return ActionResult.InvalidCoordinates;
			}

			ItemModel originItem = mergeBoard[FromX, FromY].Item;

			MergeBoardModel.MoveResultType moveResultType = island.MoveResult(
				player.GameConfig,
				originItem,
				ToX,
				ToY
			);
			if (moveResultType == MergeBoardModel.MoveResultType.Invalid
				|| moveResultType == MergeBoardModel.MoveResultType.Building) {
				return ActionResult.InvalidCoordinates;
			}

			if (commit) {
				ItemModel targetItem = mergeBoard[ToX, ToY].Item;
				if (targetItem == null || targetItem == originItem) {
					mergeBoard.MoveItem(originItem, ToX, ToY, player.ClientListener);
					player.ClientListener.OnItemMovedOnBoard(mergeBoard.Info.Type, originItem, FromX, FromY, ToX, ToY);
				} else if (originItem.CanMergeWith(targetItem, player.GameConfig)) {
					ItemModel newItem = new ItemModel(
						targetItem.Info.Type,
						targetItem.Info.Level + 1,
						player.GameConfig,
						player.CurrentTime,
						false
					);
					mergeBoard.RemoveItem(FromX, FromY, player.ClientListener);
					mergeBoard.RemoveItem(ToX, ToY, player.ClientListener);
					mergeBoard.CreateItem(ToX, ToY, newItem);
					player.ClientListener.OnItemCreatedOnBoard(mergeBoard.Info.Type, newItem, ToX, ToY, ToX, ToY, false);
					player.ClientListener.OnItemMerged(IslandId, newItem);
					player.HandleItemDiscovery(newItem);
					player.AddActivityEventScore(ActivityEventType.Merge, newItem.Info.MergeEventScore, new MergeBoardResourceContext(ToX, ToY));
					
					foreach (PlayerLiveOpsEventModel liveOpsEvent in player.LiveOpsEvents.EventModels.Values) {
						if (!(liveOpsEvent is MergeEventState mergeEventState))
							continue;
						
						mergeEventState.AddScore(newItem.Info.MergeEventScore);
						player.ClientListener.OnMergeScoreChanged(mergeEventState.MergeScore);
					}
					
					player.ServerListener.ItemMerged(newItem, newItem.Info.MergeEventScore);
					
					player.Logbook.RegisterTaskProgress(LogbookTaskType.Merge, player.CurrentTime, player.ClientListener);
					player.ProgressDailyTask(DailyTaskTypeId.Merge, 1, new MergeBoardResourceContext(ToX, ToY));
					if (targetItem.Info.Type == ChainTypeId.IslandToken) {
						player.ProgressDailyTask(DailyTaskTypeId.MergeIslandToken, 1, new MergeBoardResourceContext(ToX, ToY));
					}

					island.RunIslandTaskTriggers(player.ExecuteTrigger);
					if (island.IsCompleteBuildingFragment(player.GameConfig, newItem)) {
						island.UpdateBuildingStartState(player.GameConfig, player.HandleBuildingState);
					}

					if (newItem.BuildState == ItemBuildState.Complete) {
						foreach (ResourceInfo resource in newItem.Info.CreateRewards) {
							player.EarnResources(resource.Type, resource.Amount, IslandId, new MergeBoardResourceContext(ToX, ToY));
						}
					}

					if (targetItem.State != ItemState.Free) {
						mergeBoard.CalculateItemStates(player.HandleItemDiscovery, player.ClientListener);
						island.UpdateBuildingState(player.GameConfig, player.HandleBuildingState, player.ClientListener);
					}

					if (player.PlayerLevel >= player.GameConfig.Global.MinLevelForBubbles) {
						ItemModel bubbleItem = newItem.TrySpawnBubble(
							player.Random,
							player.GameConfig,
							player.CurrentTime
						);
						if (bubbleItem != null) {
							Coordinates coordinates = mergeBoard.FindClosestFreeTile(ToX, ToY);
							if (coordinates != null) {
								mergeBoard.CreateItem(coordinates.X, coordinates.Y, bubbleItem);
								player.ClientListener.OnItemCreatedOnBoard(
									mergeBoard.Info.Type,
									bubbleItem,
									ToX,
									ToY,
									coordinates.X,
									coordinates.Y,
									true
								);
							}
						}
					}

					if (originItem.IsWildcard && !originItem.Info.ConfigKey.Equals(targetItem.Info.ConfigKey)) {
						player.Logbook.RegisterTaskProgress(LogbookTaskType.UseBooster, player.CurrentTime, player.ClientListener);
						player.EventStream.Event(
							new PlayerBoosterItemUsed(
								originItem.Info.BoosterType,
								targetItem.Info.Type,
								targetItem.Info.Level
							)
						);
					}
				} else if (originItem.CanApplyBuilderTo(targetItem)) {
					int builderId = targetItem.BuilderId;
					if (builderId == 0) {
						builderId = player.Builders.AssignTaskToConsumable(
							IslandId,
							player.CurrentTime,
							targetItem.Info.BuildTime
						);
						targetItem.StartBuilding(builderId);
					}

					MetaTime completeAt = player.Builders.GetCompleteAt(builderId);
					MetaDuration boosterBuildTime = originItem.Booster.BuildTime;
					if (player.CurrentTime + boosterBuildTime < completeAt) {
						// Consumable builder is used entirely
						player.Builders.SetCompleteAt(builderId, completeAt - boosterBuildTime);
						originItem.Booster.BuildTime = MetaDuration.Zero;
						mergeBoard.RemoveItem(FromX, FromY, player.ClientListener);
						LevelId<ChainTypeId> destructionItem = originItem.Booster.Info.DestructionItem;
						if (destructionItem.Type != ChainTypeId.None) {
							Coordinates closestFreeTile = mergeBoard.FindClosestFreeTile(ToX, ToY);
							ItemModel destructionItemModel = new ItemModel(
								destructionItem.Type,
								destructionItem.Level,
								player.GameConfig,
								player.CurrentTime,
								true
							);
							mergeBoard.CreateItem(closestFreeTile.X, closestFreeTile.Y, destructionItemModel);
							player.ClientListener.OnItemCreatedOnBoard(
								IslandId,
								destructionItemModel,
								ToX,
								ToY,
								closestFreeTile.X,
								closestFreeTile.Y,
								false
							);
						}
					} else {
						// Consumable builder is not used entirely
						originItem.Booster.BuildTime -= completeAt - player.CurrentTime;
						player.Builders.SetCompleteAt(builderId, player.CurrentTime);
						targetItem.FinishBuilding();
						mergeBoard.RemoveItem(FromX, FromY, player.ClientListener);
						Coordinates closestFreeTile = mergeBoard.FindClosestFreeTile(ToX, ToY);
						mergeBoard.CreateItem(closestFreeTile.X, closestFreeTile.Y, originItem);
						player.ClientListener.OnItemCreatedOnBoard(
							IslandId,
							originItem,
							ToX,
							ToY,
							closestFreeTile.X,
							closestFreeTile.Y,
							false
						);
					}
					player.Logbook.RegisterTaskProgress(LogbookTaskType.UseBooster, player.CurrentTime, player.ClientListener);
				} else {
					if (targetItem.Converter == null) {
						mergeBoard.MoveItem(originItem, ToX, ToY, player.ClientListener);
						Coordinates targetCoordinates = mergeBoard.FindClosestFreeTile(ToX, ToY);
						// Note, targetCoordinates cannot be null as we have just released the origin.
						mergeBoard.MoveItem(targetItem, targetCoordinates.X, targetCoordinates.Y, player.ClientListener);
						player.ClientListener.OnItemMovedOnBoard(
							mergeBoard.Info.Type, targetItem, ToX, ToY, targetCoordinates.X, targetCoordinates.Y);
						player.ClientListener.OnItemMovedOnBoard(mergeBoard.Info.Type, originItem, FromX, FromY, ToX, ToY);
					} else {
						if (targetItem.CanConvertItem(originItem)) {
							mergeBoard.RemoveItem(FromX, FromY, player.ClientListener);
							targetItem.Converter.ConvertItem(originItem, targetItem.Creator);
						} else {
							player.ClientListener.OnItemMovedOnBoard(mergeBoard.Info.Type, originItem, FromX, FromY, FromX, FromY);
						}
					}
				}
			}

			return ActionResult.Success;
		}
	}
}
