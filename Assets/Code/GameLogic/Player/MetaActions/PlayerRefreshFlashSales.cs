using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerRefreshFlashSales)]
	public class PlayerRefreshFlashSales : PlayerAction {
		public PlayerRefreshFlashSales() { }

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			ShopInfo info = player.GameConfig.Shop;
			if (!player.Wallet.EnoughCurrency(info.RefreshCost.Type, info.RefreshCost.Amount)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				int timeLeft = F64.CeilToInt((player.Market.NextRefreshAt - player.CurrentTime).ToSecondsF64());
				player.Market.Refresh(player);
				player.ConsumeResources(info.RefreshCost.Type, info.RefreshCost.Amount, ResourceModificationContext.Empty);
				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.FlashSalesRefreshed,
						info.RefreshCost.Type,
						info.RefreshCost.Amount,
						"",
						0,
						CurrencyTypeId.Time,
						timeLeft,
						""
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
