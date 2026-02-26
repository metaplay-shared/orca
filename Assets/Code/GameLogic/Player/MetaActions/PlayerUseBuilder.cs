using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerUseBuilder)]
	public class PlayerUseBuilder : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerUseBuilder() { }

		public PlayerUseBuilder(IslandTypeId islandId, int x, int y) {
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

			if (item.State != ItemState.Free || !mergeBoard.LockArea.IsFree(X, Y)) {
				return ActionResult.InvalidState;
			}

			if (item.BuildState != ItemBuildState.NotStarted) {
				return ActionResult.InvalidState;
			}

			if (player.Builders.Free <= 0) {
				return ActionResult.NoBuildersAvailable;
			}

			if (commit) {
				F64 discountFactor = player.BuilderTimerFactor();

				MetaDuration buildTime =
					MetaDuration.FromSeconds(F64.CeilToInt(discountFactor * item.Info.BuildTime.ToSecondsF64()));
				int builderId = player.Builders.AssignTask(IslandId, player.CurrentTime, buildTime);
				item.StartBuilding(builderId);
				player.ClientListener.OnMergeItemStateChanged(IslandId, item);
				player.ClientListener.OnBuilderStateChanged();
				player.Logbook.RegisterTaskProgress(LogbookTaskType.UseBuilder, player.CurrentTime, player.ClientListener);

				int timeLeft = F64.CeilToInt((player.Builders.GetCompleteAt(builderId) - player.CurrentTime).ToSecondsF64());
				player.EventStream.Event(
					new PlayerBuilderUsed(IslandId, item.Info.Type, item.Info.Level, BuilderActionId.Build, timeLeft)
				);
			}

			return ActionResult.Success;
		}
	}
}
