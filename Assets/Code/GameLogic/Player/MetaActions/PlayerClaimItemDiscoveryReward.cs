using System.Collections.Generic;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerClaimItemDiscoveryReward)]
	public class PlayerClaimItemDiscoveryReward : PlayerAction {
		public LevelId<ChainTypeId> ItemId { get; private set; }

		public PlayerClaimItemDiscoveryReward() { }

		public PlayerClaimItemDiscoveryReward(LevelId<ChainTypeId> itemId) {
			ItemId = itemId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.GameConfig.Chains.ContainsKey(ItemId)) {
				return ActionResult.InvalidParam;
			}

			DiscoveryState state = player.Merge.ItemDiscovery.GetState(ItemId);
			if (state != DiscoveryState.Discovered) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				player.Merge.ItemDiscovery.MarkClaimed(ItemId, player.CurrentTime);
				player.ClientListener.OnItemDiscoveryChanged(ItemId);
				foreach (ResourceInfo resource in player.GameConfig.Chains[ItemId].DiscoveryRewards) {
					player.EarnResources(
						resource.Type,
						resource.Amount,
						IslandTypeId.None,
						new DiscoveryResourceContext(ItemId.Type, ItemId.Level)
					);
				}
				player.EventStream.Event(new PlayerItemDiscoveryRewardClaimed(ItemId.Type, ItemId.Level));
			}

			return ActionResult.Success;
		}
	}
}
