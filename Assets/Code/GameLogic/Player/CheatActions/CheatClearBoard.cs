using System.Collections.Generic;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(CheatActionCodes.CheatClearBoard)]
	[DevelopmentOnlyAction]
	public class CheatClearBoard : PlayerAction {
		public IslandTypeId Island { get; private set; }

		public CheatClearBoard() { }

		public CheatClearBoard(IslandTypeId island) {
			Island = island;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(Island)) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[Island];
			if (island.State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			MergeBoardModel mergeBoard = island.MergeBoard;

			if (commit) {
				List<ItemModel> items = new List<ItemModel>(mergeBoard.Items);
				foreach (ItemModel item in items) {
					mergeBoard.RemoveItem(item.X, item.Y, player.ClientListener);
				}
			}

			return ActionResult.Success;
		}
	}
}
