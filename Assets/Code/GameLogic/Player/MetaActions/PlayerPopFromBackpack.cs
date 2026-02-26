using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerPopFromBackpack)]
	public class PlayerPopFromBackpack : PlayerAction {
		public IslandTypeId Island { get; private set; }
		public int Index { get; private set; }

		public PlayerPopFromBackpack() { }

		public PlayerPopFromBackpack(IslandTypeId island, int index) {
			Island = island;
			Index = index;
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

			if (Index < 0 || Index >= player.Backpack.Items.Count) {
				return ActionResult.InvalidIndex;
			}

			Coordinates coordinates = mergeBoard.FindClosestFreeTile(0, 0);
			if (coordinates == null) {
				return ActionResult.NotEnoughSpace;
			}

			if (commit) {
				ItemModel item = player.Backpack.Items[Index];
				player.Backpack.Items.RemoveAt(Index);
				player.ClientListener.OnItemRemovedFromBackpack(Index, item);
				mergeBoard.CreateItem(coordinates.X, coordinates.Y, item);
				player.ClientListener.OnItemCreatedOnBoard(
					Island,
					item,
					StaticConfig.BackpackX,
					StaticConfig.BackpackY,
					coordinates.X,
					coordinates.Y,
					true
				);
				player.EventStream.Event(
					new PlayerPoppedFromBackpack(Island, item.Info.Type, item.Info.Level)
				);
			}

			return ActionResult.Success;
		}
	}
}
