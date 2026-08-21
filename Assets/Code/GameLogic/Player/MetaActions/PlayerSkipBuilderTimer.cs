using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSkipBuilderTimer)]
	public class PlayerSkipBuilderTimer : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerSkipBuilderTimer() { }

		public PlayerSkipBuilderTimer(IslandTypeId islandId, int x, int y) {
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

			if (!item.IsUsingBuilder) {
				return ActionResult.InvalidState;
			}

			MetaTime completionTime = player.Builders.GetCompleteAt(item.UsedBuilderId);
			if (completionTime <= player.CurrentTime) {
				return ActionResult.Success;
			}

			int timeLeft = F64.CeilToInt((completionTime - player.CurrentTime).ToSecondsF64());
			Cost cost = player.SkipBuilderTimerCost(item.UsedBuilderId);

			if (!player.Wallet.EnoughCurrency(cost.Type, cost.Amount)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				player.ConsumeResources(cost.Type, cost.Amount, ResourceModificationContext.Empty);
				player.Builders.Reset(item.UsedBuilderId);
				if (item.BuildState == ItemBuildState.Building) {
					item.FinishBuilding();
				} else {
					item.Mine.UpdateState(player.GameConfig);
				}
				player.ClientListener.OnBuilderStateChanged();
				player.ClientListener.OnMergeItemStateChanged(IslandId, item);

				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.BuilderTimerSkipped,
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
