using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerOpenMergeItem)]
	public class PlayerOpenMergeItem : PlayerAction {
		public IslandTypeId Island { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerOpenMergeItem() { }

		public PlayerOpenMergeItem(IslandTypeId island, int x, int y) {
			Island = island;
			X = x;
			Y = y;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(Island)) {
				return ActionResult.InvalidParam;
			}

			if (player.Islands[Island].State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			IslandModel island = player.Islands[Island];
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

			if (!mergeBoard.CanMoveFrom(X, Y) || item.LockedState != ItemLockedState.Closed) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				item.StartOpening(player.CurrentTime);
				player.ClientListener.OnMergeItemStateChanged(Island, item);
				player.EventStream.Event(
					new PlayerMergeItemOpened(Island, item.Info.Type, item.Info.Level, item.OpenAt - player.CurrentTime)
				);
			}

			return ActionResult.Success;
		}
	}
}
