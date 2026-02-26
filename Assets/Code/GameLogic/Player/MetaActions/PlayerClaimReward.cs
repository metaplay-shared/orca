using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerClaimReward)]
	public class PlayerClaimReward : PlayerAction {
		public IslandTypeId IslandId { get; private set; }

		public PlayerClaimReward() { }

		public PlayerClaimReward(IslandTypeId islandId) {
			IslandId = islandId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (player.Rewards.Count == 0) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				RewardModel reward = player.Rewards[0];
				player.Rewards.RemoveAt(0);

				foreach (ResourceInfo resource in reward.Resources) {
					player.EarnResources(resource.Type, resource.Amount, IslandId, ResourceModificationContext.Empty);
				}

				if (reward.Items.Count > 0) {
					ItemModel rewardChest = new ItemModel(
						reward.ChestType,
						reward.ChestLevel,
						player.GameConfig,
						player.CurrentTime,
						true
					);
					foreach (ItemCountInfo item in reward.Items) {
						for (int i = 0; i < item.Count; i++) {
							rewardChest.Creator.ItemQueue.Add(item.ChainId);
						}
					}

					player.AddItemToHolder(IslandTypeId.MainIsland, rewardChest);
				}
				player.EventStream.Event(new PlayerRewardClaimed(reward.Metadata.Type, reward.Items.Count));

				player.ClientListener.OnRewardClaimed();
			}

			return ActionResult.Success;
		}
	}
}
