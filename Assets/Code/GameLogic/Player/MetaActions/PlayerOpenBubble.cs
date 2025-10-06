using System;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerOpenBubble)]
	public class PlayerOpenBubble : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerOpenBubble() { }

		public PlayerOpenBubble(IslandTypeId islandId, int x, int y) {
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

			if (!item.Bubble) {
				return ActionResult.InvalidState;
			}

			int cost = item.Info.BubblePrice;
			if (!player.Wallet.EnoughCurrency(CurrencyTypeId.Gems, cost)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				player.ConsumeResources(CurrencyTypeId.Gems, cost, ResourceModificationContext.Empty);
				item.OpenBubble();
				player.ClientListener.OnMergeItemStateChanged(IslandId, item);

				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.BubbleOpened,
						CurrencyTypeId.Gems,
						cost,
						"",
						0,
						CurrencyTypeId.Time,
						0,
						item.Info.Type.Value
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
