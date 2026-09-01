using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSkipOpenItemTimer)]
	public class PlayerSkipOpenItemTimer : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerSkipOpenItemTimer() { }

		public PlayerSkipOpenItemTimer(IslandTypeId islandId, int x, int y) {
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

			if (item.LockedState != ItemLockedState.Opening) {
				return ActionResult.InvalidState;
			}

			Cost cost = item.SkipOpenCost(player.GameConfig, player.CurrentTime);
			if (!player.Wallet.EnoughCurrency(cost.Type, cost.Amount)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				int timeLeft = F64.RoundToInt((item.OpenAt - player.CurrentTime).ToSecondsF64());
				player.ConsumeResources(cost.Type, cost.Amount, ResourceModificationContext.Empty);
				item.OpenNow();
				player.ClientListener.OnMergeItemStateChanged(IslandId, item);
				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.OpenItemTimerSkipped,
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
