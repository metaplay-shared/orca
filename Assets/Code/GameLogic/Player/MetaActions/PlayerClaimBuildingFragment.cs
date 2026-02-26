using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerClaimBuildingFragment)]
	public class PlayerClaimBuildingFragment : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerClaimBuildingFragment() { }

		public PlayerClaimBuildingFragment(IslandTypeId islandId, int x, int y) {
			IslandId = islandId;
			X = x;
			Y = y;
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

			if (!item.CanMove) {
				return ActionResult.InvalidState;
			}

			if (!island.CanClaimAsBuilding(player.GameConfig, item)) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				mergeBoard.RemoveItem(X, Y, player.ClientListener);
				if (island.BuildingSlotsComplete(player.GameConfig)) {
					island.UseBuildingFragment(player.GameConfig, item.Info, player.AddReward, player.ClientListener, player.ServerListener);
				} else {
					int index = island.MarkBuildingFragmentDone(player.GameConfig, item.Info.Type);
					if (index >= 0) {
						LevelId<IslandTypeId> fragmentId = new LevelId<IslandTypeId>(IslandId, index);
						if (player.GameConfig.BuildingFragments.ContainsKey(fragmentId)) {
							BuildingFragmentInfo fragmentInfo = player.GameConfig.BuildingFragments[fragmentId];
							RewardMetadata metadata = new RewardMetadata {
								Type = RewardType.BuildingFragment,
								Level = index + 1,
								Island = IslandId
							};

							if (fragmentInfo.RewardItems.Count > 0) {
								RewardModel rewards = new RewardModel(
									fragmentInfo.RewardResources,
									fragmentInfo.RewardItems,
									ChainTypeId.LevelUpRewards,
									1,
									metadata
								);
								player.AddReward(rewards);
							}

							foreach (TriggerId trigger in fragmentInfo.Triggers) {
								player.Triggers.ExecuteTrigger(player, trigger);
							}
						}
					}
					player.ClientListener.OnBuildingFragmentCollected(IslandId, item, X, Y);
					island.UpdateBuildingState(player.GameConfig, player.HandleBuildingState, player.ClientListener);
				}

				player.EventStream.Event(
					new PlayerBuildingFragmentClaimed(
						IslandId,
						island.BuildingState,
						island.BuildingLevel.Level,
						item.Info.Type
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
