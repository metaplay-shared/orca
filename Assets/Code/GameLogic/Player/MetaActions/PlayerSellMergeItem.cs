using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSellMergeItem)]
	public class PlayerSellMergeItem : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerSellMergeItem() { }

		public PlayerSellMergeItem(IslandTypeId islandId, int x, int y) {
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

			if (!item.CanMove || !item.Info.Sellable) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				mergeBoard.RemoveItem(X, Y, player.ClientListener);
				if (item.Info.SellPrice > 0) {
					player.EarnResources(
						CurrencyTypeId.Gold,
						item.Info.SellPrice,
						IslandId,
						new MergeBoardResourceContext(X, Y)
					);
					player.EventStream.Event(
						new PlayerEconomyAction(
							player,
							EconomyActionId.ItemSold,
							CurrencyTypeId.None,
							0,
							"",
							0,
							CurrencyTypeId.Gold,
							item.Info.SellPrice,
							item.Info.Type.Value
						)
					);
				}
			}

			return ActionResult.Success;
		}
	}
}
