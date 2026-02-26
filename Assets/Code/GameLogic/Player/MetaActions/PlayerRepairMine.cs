using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerRepairMine)]
	public class PlayerRepairMine : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerRepairMine() { }

		public PlayerRepairMine(IslandTypeId islandId, int x, int y) {
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

			if (player.Builders.Free <= 0) {
				return ActionResult.NoBuildersAvailable;
			}

			if (item.Mine.State != MineState.NeedsRepair) {
				return ActionResult.InvalidState;
			}

			if (item.Mine.Queue.Count > 0) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				int builderId = player.Builders.AssignTask(IslandId, player.CurrentTime, item.Mine.RepairTime);
				item.Mine.StartRepairing(builderId);
				player.ClientListener.OnMergeItemStateChanged(IslandId, item);
				player.ClientListener.OnBuilderStateChanged();
				int timeLeft = F64.CeilToInt((player.Builders.GetCompleteAt(builderId) - player.CurrentTime).ToSecondsF64());
				player.Logbook.RegisterTaskProgress(LogbookTaskType.RepairMine, player.CurrentTime, player.ClientListener);

				player.EventStream.Event(
					new PlayerBuilderUsed(IslandId, item.Info.Type, item.Mine.Info.Level, BuilderActionId.Mine, timeLeft)
				);
			}

			return ActionResult.Success;
		}
	}
}
