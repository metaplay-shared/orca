using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerCollectMergeItem)]
	public class PlayerCollectMergeItem : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerCollectMergeItem() { }

		public PlayerCollectMergeItem(IslandTypeId islandId, int x, int y) {
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

			MergeBoardModel mergeBoard = player.Islands[IslandId].MergeBoard;
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

			if (!item.CanCollect(IslandId)) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				mergeBoard.RemoveItem(X, Y, player.ClientListener);
				mergeBoard.CalculateItemStates(player.HandleItemDiscovery, player.ClientListener);
				if (item.Info.TargetIsland == IslandId || item.Info.TargetIsland == IslandTypeId.All) {
					player.EarnResources(
						item.Info.CollectableType,
						item.Info.CollectableValue,
						IslandId,
						new MergeBoardResourceContext(X, Y)
					);
					foreach (TriggerId trigger in item.Info.CollectedTriggers) {
						player.Triggers.ExecuteTrigger(player, trigger);
					}

					if (item.Info.CollectableType.WalletResource) {
						player.EventStream.Event(
							new PlayerEconomyAction(
								player,
								EconomyActionId.ItemCollected,
								CurrencyTypeId.None,
								0,
								"",
								0,
								item.Info.CollectableType,
								item.Info.CollectableValue,
								""
							)
						);
					} else if (player.Heroes.UnlocksHero(player.GameConfig, item)) {
						int count = 0;
						foreach (IslandModel isl in player.Islands.Values) {
							if (isl.MergeBoard != null) {
								isl.MergeBoard?.ReplaceItems(
									player.GameConfig,
									player.CurrentTime,
									ReplacementContextId.UnlockHero,
									player.ClientListener
								);
								count += isl.MergeBoard?.RemoveItems(
									player.Heroes.CurrentHeroItem,
									-item.Info.Level,
									1000,
									true,
									player.ClientListener
								) ?? 0;
								isl.MergeBoard.CalculateItemStates(player.HandleItemDiscovery, player.ClientListener);
							}
						}
						player.EventStream.Event(new PlayerHeroUnlocked(player.Heroes.CurrentHero, count));
						player.UnlockHero();
					} else {
						player.EventStream.Event(
							new PlayerMergeItemCollected(
								item.Info.Type,
								item.Info.Level,
								item.Info.CollectableType,
								item.Info.CollectableValue
							)
						);
					}
					player.Logbook.RegisterTaskProgress(LogbookTaskType.CollectItem, item.Info, player.CurrentTime, player.ClientListener);
				} else {
					player.AddItemToHolder(item.Info.TargetIsland, item);
					if (item.IsUsingBuilder) {
						BuilderModel builder = player.Builders.GetBuilder(item.UsedBuilderId);
						if (builder != null) {
							builder.Island = item.Info.TargetIsland;
						}
					}
					player.ClientListener.OnItemTransferredToIsland(item.Info.TargetIsland, item, X, Y);
				}
				player.Islands[IslandId].UpdateBuildingState(player.GameConfig, player.HandleBuildingState, player.ClientListener);
			}

			return ActionResult.Success;
		}
	}
}
