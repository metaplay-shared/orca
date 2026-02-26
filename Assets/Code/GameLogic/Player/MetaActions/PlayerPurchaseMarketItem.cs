using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerPurchaseMarketItem)]
	public class PlayerPurchaseMarketItem : PlayerAction {
		public ShopCategoryId Category { get; private set; }
		public int Index { get; private set; }
		public IslandTypeId IslandId { get; private set; }

		public PlayerPurchaseMarketItem() { }

		public PlayerPurchaseMarketItem(ShopCategoryId category, int index, IslandTypeId islandId) {
			Category = category;
			Index = index;
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
			if (island.State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			LevelId<ShopCategoryId> shopItemId = new(Category, Index);
			MarketItemModel item = player.Market.GetMarketItem(shopItemId);
			if (item == null) {
				return ActionResult.InvalidParam;
			}

			if (item.Info.ItemType != ChainTypeId.None && item.ItemsLeft <= 0) {
				return ActionResult.InvalidState;
			}

			ChainTypeId itemType = player.MapRewardToRealType(IslandId, item.Info.ItemType);
			if (item.Info.ItemType != ChainTypeId.None && itemType == ChainTypeId.None) {
				return ActionResult.InvalidParam;
			}

			int cost = item.Info.Cost;
			if (!player.Wallet.EnoughCurrency(item.Info.CostType, cost)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				player.ConsumeResources(item.Info.CostType, cost, new FlashSaleResourceContext(Category, Index));
				if (itemType == ChainTypeId.None) {
					player.Wallet.Currency(item.Info.CurrencyType).Purchase(item.Info.Count);
					player.ClientListener.OnResourcesModified(
						item.Info.CurrencyType,
						item.Info.Count,
						new MarketResourceContext(Category, Index)
					);
				} else {
					for (int i = 0; i < item.Info.Count; i++) {
						player.AddItemToHolder(
							IslandId,
							new ItemModel(itemType, item.Info.ItemLevel, player.GameConfig, player.CurrentTime, true)
						);
					}
					item.ItemsLeft--;
				}
				player.ClientListener.OnMarketItemUpdated(shopItemId);
				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.FlashSaleItemPurchased,
						item.Info.CostType,
						cost,
						"",
						0,
						CurrencyTypeId.None,
						0,
						itemType.Value + ":" + item.Info.ItemLevel
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
