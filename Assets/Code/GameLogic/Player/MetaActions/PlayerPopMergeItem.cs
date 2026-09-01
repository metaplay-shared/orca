using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerPopMergeItem)]
	public class PlayerPopMergeItem : PlayerAction {
		public IslandTypeId IslandId { get; private set; }

		public PlayerPopMergeItem() { }

		public PlayerPopMergeItem(IslandTypeId islandId) {
			IslandId = islandId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(IslandId)) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[IslandId];

			if (island.State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			MergeBoardModel mergeBoard = island.MergeBoard;

			if (mergeBoard.ItemHolder.Count <= 0) {
				return ActionResult.NotEnoughResources;
			}

			Coordinates coordinates = mergeBoard.FindClosestFreeTile(0, 0);
			if (coordinates == null) {
				// No free tiles, nothing to do. This is not really an error.
				return ActionResult.Success;
			}
			if (commit) {
				ItemModel item = mergeBoard.ItemHolder[0];
				player.RemoveFromItemHolder(IslandId, item);
				mergeBoard.CreateItem(coordinates.X, coordinates.Y, item);
				player.ClientListener.OnItemCreatedOnBoard(
					IslandId,
					item,
					StaticConfig.ItemHolderX,
					StaticConfig.ItemHolderY,
					coordinates.X,
					coordinates.Y,
					true
				);
				foreach (TriggerId trigger in item.Info.DiscoveredTriggers) {
					player.Triggers.ExecuteTrigger(player, trigger);
				}
				island.RunIslandTaskTriggers(player.ExecuteTrigger);
				player.EventStream.Event(
					new PlayerMergeItemPopped(IslandId, item.Info.Type, item.Info.Level, mergeBoard.ItemHolder.Count)
				);
			}
			return ActionResult.Success;
		}
	}
}
