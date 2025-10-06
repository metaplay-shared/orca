using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(CheatActionCodes.CheatAddMergeItem)]
	[DevelopmentOnlyAction]
	public class CheatAddMergeItem : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public ChainTypeId Type { get; private set; }
		public int Level { get; private set; }

		public CheatAddMergeItem() { }

		public CheatAddMergeItem(IslandTypeId islandId, ChainTypeId type, int level) {
			IslandId = islandId;
			Type = type;
			Level = level;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.GameConfig.Chains.ContainsKey(new LevelId<ChainTypeId>(Type, Level))) {
				return ActionResult.InvalidParam;
			}

			if (!player.Islands.ContainsKey(IslandId)) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[IslandId];
			if (island.State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			MergeBoardModel mergeBoard = island.MergeBoard;
			Coordinates coordinates = mergeBoard.FindClosestFreeTile(0, 0);
			if (coordinates == null) {
				return ActionResult.InvalidCoordinates;
			}

			if (commit) {
				ItemModel item = new ItemModel(Type, Level, player.GameConfig, player.CurrentTime, false);
				mergeBoard.CreateItem(coordinates.X, coordinates.Y, item);
				player.ClientListener.OnItemCreatedOnBoard(
					IslandId,
					item,
					0,
					0,
					coordinates.X,
					coordinates.Y,
					false
				);
				player.HandleItemDiscovery(item);

				if (island.IsCompleteBuildingFragment(player.GameConfig, item)) {
					island.UpdateBuildingStartState(player.GameConfig, player.HandleBuildingState);
				}
			}

			return ActionResult.Success;
		}
	}
}
