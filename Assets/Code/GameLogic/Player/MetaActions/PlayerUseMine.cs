using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerUseMine)]
	public class PlayerUseMine : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerUseMine() { }

		public PlayerUseMine(IslandTypeId islandId, int x, int y) {
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

			if (item.Mine.Info.RequiresBuilder && player.Builders.Free <= 0) {
				return ActionResult.NoBuildersAvailable;
			}

			if (item.Mine.State != MineState.Idle) {
				return ActionResult.InvalidState;
			}

			if (player.Merge.Energy.ProducedAtUpdate < item.Mine.EnergyUsage) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				MetaDuration duration =	(item.Mine.MineCycle < item.Mine.Info.MineCycles - 1
						? item.Mine.Info.MiningTime
						: item.Mine.Info.LastCycleMiningTime);
				int builderId = item.Mine.Info.RequiresBuilder
					? player.Builders.AssignTask(IslandId, player.CurrentTime, duration)
					: player.Builders.AssignTaskToConsumable(IslandId, player.CurrentTime, duration);
				item.Mine.StartMining(builderId);
				item.Mine.CreateItems(player.Random);
				player.ClientListener.OnMergeItemStateChanged(IslandId, item);
				player.ClientListener.OnBuilderStateChanged();
				int timeLeft = F64.CeilToInt((player.Builders.GetCompleteAt(builderId) - player.CurrentTime).ToSecondsF64());
				player.ClientListener.OnBuilderUsed(IslandId, item, timeLeft);
				if (item.Mine.EnergyUsage > 0) {
					player.ConsumeResources(
						CurrencyTypeId.Energy,
						item.Mine.EnergyUsage,
						ResourceModificationContext.Empty
					);
				}

				if (item.IsPermanentMine) {
					player.Logbook.RegisterTaskProgress(
						LogbookTaskType.UseMine,
						player.CurrentTime,
						player.ClientListener
					);
				}

				if (item.Mine.Info.Chest) {
					player.ProgressDailyTask(DailyTaskTypeId.OpenChest, 1, new MergeBoardResourceContext(X, Y));
					player.AddActivityEventScore(
						ActivityEventType.Chests,
						item.Mine.Info.ChestEventScore,
						new MergeBoardResourceContext(X, Y)
					);
					player.Logbook.RegisterTaskProgress(
						LogbookTaskType.OpenChest,
						player.CurrentTime,
						player.ClientListener
					);
				}

				player.EventStream.Event(
					new PlayerBuilderUsed(IslandId, item.Info.Type, item.Mine.Info.Level, BuilderActionId.Mine, timeLeft)
				);
			}

			return ActionResult.Success;
		}
	}
}
