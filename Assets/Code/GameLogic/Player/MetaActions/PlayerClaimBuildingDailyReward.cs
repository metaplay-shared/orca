using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerClaimBuildingDailyReward)]
	public class PlayerClaimBuildingDailyReward : PlayerAction {
		public IslandTypeId IslandId { get; private set; }

		public PlayerClaimBuildingDailyReward() { }

		public PlayerClaimBuildingDailyReward(IslandTypeId islandId) {
			IslandId = islandId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(IslandId)) {
				return ActionResult.InvalidParam;
			}

			if (player.Islands[IslandId].State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			IslandModel island = player.Islands[IslandId];
			if (!island.BuildingDailyRewardAvailable(player.GameConfig, player.CurrentTime)) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				island.MarkBuildingDailyRewardClaimed(player.CurrentTime);
				BuildingLevelInfo levelInfo =
					player.GameConfig.BuildingLevels[new LevelId<IslandTypeId>(IslandId, island.BuildingLevel.Level)];
				RewardModel reward = new RewardModel(
					levelInfo.DailyRewardResources,
					levelInfo.DailyRewardItems,
					ChainTypeId.IslandRewards,
					1,
					new RewardMetadata { Type = RewardType.BuildingDaily, Island = IslandId }
				);
				player.AddReward(reward);

				player.EventStream.Event(new PlayerBuildingDailyRewardClaimed(IslandId));
			}

			return ActionResult.Success;
		}
	}
}
