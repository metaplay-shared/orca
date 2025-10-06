using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerEnterMap)]
	public class PlayerEnterMap : PlayerAction {
		public IslandTypeId FromIsland { get; private set; }

		public PlayerEnterMap() { }

		public PlayerEnterMap(IslandTypeId fromIsland) {
			FromIsland = fromIsland;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (commit) {
				foreach (MapTriggerInfo triggerInfo in player.GameConfig.MapTriggers.Values) {
					if (HasEnoughResources(player, triggerInfo.Resource) &&
						HasUnlockedIsland(player, triggerInfo.UnlockedIsland)) {
						player.Triggers.ExecuteTrigger(player, triggerInfo.Trigger);
					}
				}
				player.UpdateVipPassLockAreas();
				player.EventStream.Event(new PlayerMapEntered(FromIsland));
				player.CurrentIsland = IslandTypeId.None;
				player.LastIsland = FromIsland;
			}

			return ActionResult.Success;
		}

		private bool HasEnoughResources(PlayerModel player, ResourceInfo resources) {
			if (resources.Type == CurrencyTypeId.None) {
				return true;
			}
			if (resources.Type.WalletResource) {
				return player.Wallet.EnoughCurrency(resources.Type, resources.Amount);
			}

			return player.Inventory.Resources.GetValueOrDefault(resources.Type) >= resources.Amount;
		}

		private bool HasUnlockedIsland(PlayerModel player, IslandTypeId island) {
			if (island == IslandTypeId.None) {
				return true;
			}

			if (!player.Islands.ContainsKey(island)) {
				return false;
			}

			return player.Islands[island].State != IslandState.Hidden;
		}
	}
}
