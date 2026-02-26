using System;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSkipCreatorTimer)]
	public class PlayerSkipCreatorTimer : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerSkipCreatorTimer() { }

		public PlayerSkipCreatorTimer(IslandTypeId islandId, int x, int y) {
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

			if (item.Creator == null) {
				return ActionResult.InvalidState;
			}

			int timeLeft = F64.CeilToInt(item.Creator.TimeToFill(player.CurrentTime).ToSecondsF64());
			Cost cost = item.SkipCreatorTimerCost(player.GameConfig, player.CurrentTime);

			if (!player.Wallet.EnoughCurrency(cost.Type, cost.Amount)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				player.ConsumeResources(cost.Type, cost.Amount, ResourceModificationContext.Empty);
				item.Creator.Producer.Reset(player.CurrentTime, item.Creator.Info.ItemCount);
				player.ClientListener.OnMergeItemStateChanged(IslandId, item);

				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.CreatorTimerSkipped,
						cost.Type,
						cost.Amount,
						"",
						0,
						CurrencyTypeId.Time,
						timeLeft,
						item.Info.Type.Value
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
