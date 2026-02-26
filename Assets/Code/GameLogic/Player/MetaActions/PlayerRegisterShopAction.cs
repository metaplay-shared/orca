using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerRegisterShopOpen)]
	public class PlayerRegisterShopOpenAction : PlayerAction
	{
		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (commit) {
				if (!player.Wallet.HasOpenedShop) {
					player.ExecuteTrigger(player.GameConfig.Shop.FirstOpenTrigger);
					player.Wallet.HasOpenedShop = true;
				}
			}

			return MetaActionResult.Success;
		}
	}

	[ModelAction(ActionCodes.PlayerRegisterShopClose)]
	public class PlayerRegisterShopCloseAction : PlayerAction
	{
		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (commit) {
				if (!player.Wallet.HasClosedShop) {
					player.ExecuteTrigger(player.GameConfig.Shop.FirstCloseTrigger);
					player.Wallet.HasClosedShop = true;
				}
			}

			return MetaActionResult.Success;
		}
	}
}
