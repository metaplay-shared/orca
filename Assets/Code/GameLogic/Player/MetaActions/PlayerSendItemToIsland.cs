using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSendItemToIsland)]
	public class PlayerSendItemToIsland : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int FromX { get; private set; }
		public int FromY { get; private set; }
		public IslandTypeId TargetIslandId { get; private set; }

		public PlayerSendItemToIsland() { }

		public PlayerSendItemToIsland(IslandTypeId islandId, int fromX, int fromY, IslandTypeId targetIslandId) {
			IslandId = islandId;
			FromX = fromX;
			FromY = fromY;
			TargetIslandId = targetIslandId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(IslandId) ||
				!player.Islands.ContainsKey(TargetIslandId) ||
				TargetIslandId == IslandTypeId.EnergyIsland ||
				IslandId == TargetIslandId) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[IslandId];
			IslandModel targetIsland = player.Islands[TargetIslandId];
			if (island.State != IslandState.Open || targetIsland.State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			MergeBoardModel mergeBoard = island.MergeBoard;
			if (!mergeBoard.CanMoveFrom(FromX, FromY)) {
				return ActionResult.InvalidCoordinates;
			}

			ItemModel item = mergeBoard[FromX, FromY].Item;
			if (!item.Info.Transferable) {
				return ActionResult.InvalidParam;
			}

			if (commit) {
				mergeBoard.RemoveItem(FromX, FromY, player.ClientListener);
				player.AddItemToHolder(TargetIslandId, item);
			}

			return ActionResult.Success;
		}
	}
}
