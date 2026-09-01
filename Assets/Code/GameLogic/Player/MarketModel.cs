using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class MarketModel {
		[MetaMember(1)] public MetaTime NextRefreshAt { get; private set; }
		[MetaMember(2)] public List<MarketItemModel> MarketItems { get; private set; }

		public MarketModel() {
			NextRefreshAt = MetaTime.Epoch;
			MarketItems = new List<MarketItemModel>();
		}

		public bool InterestingItems {
			get {
				foreach (MarketItemModel item in MarketItems) {
					if (item.Info.CostType == CurrencyTypeId.None && item.ItemsLeft > 0) {
						return true;
					}
				}

				return false;
			}
		}

		public void Update(PlayerModel player) {
			if (player.CurrentTime >= NextRefreshAt) {
				Refresh(player);
			}
		}

		public void Refresh(PlayerModel player) {
			MarketItems.Clear();
			foreach (MarketItemInfo item in player.GameConfig.MarketItems.Values) {
				MarketItems.Add(new MarketItemModel(item));
			}

			NextRefreshAt = player.CurrentTime + player.GameConfig.Shop.RefreshInterval;
			player.ClientListener.OnShopUpdated();
		}

		public MetaDictionary<ShopCategoryId, List<MarketItemModel>> GetMarketItems(
			PlayerModel player,
			IslandTypeId island
		) {
            MetaDictionary<ShopCategoryId, List<MarketItemModel>> items = new();
			foreach (MarketItemModel item in MarketItems) {
				if (IsValidMarketItem(player, island, item)) {
					List<MarketItemModel> itemList = items.GetValueOrDefault(item.Info.Category);
					if (itemList == null) {
						itemList = new List<MarketItemModel>();
						items[item.Info.Category] = itemList;
					}

					itemList.Add(item);
				}
			}

			return items;
		}

		private bool IsValidMarketItem(PlayerModel player, IslandTypeId island, MarketItemModel item) {
			if (item.Info.CurrencyType != CurrencyTypeId.None) {
				return true;
			}
			ChainTypeId realType = player.MapRewardToRealTypeUI(island, item.Info.ItemType);
			if (realType != ChainTypeId.None) {
				ChainInfo itemInfo = player.GameConfig.Chains[new LevelId<ChainTypeId>(realType, item.Info.ItemLevel)];
				if (itemInfo.TargetIsland == island || itemInfo.TargetIsland == IslandTypeId.All) {
					return true;
				}
			}

			return false;
		}

		public MarketItemModel GetMarketItem(LevelId<ShopCategoryId> shopItemId) {
			foreach (MarketItemModel item in MarketItems) {
				if (item.Info.ConfigKey.Equals(shopItemId)) {
					return item;
				}
			}

			return null;
		}
	}

	[MetaSerializable]
	public class MarketItemModel {
		[MetaMember(1)] public MarketItemInfo Info { get; private set; }
		[MetaMember(2)] public int ItemsLeft { get; set; }

		public MarketItemModel() { }

		public MarketItemModel(MarketItemInfo info) {
			Info = info;
			ItemsLeft = info.Available;
		}

		public int GetCost(
			SharedGameConfig gameConfig,
			Func<IslandTypeId, ChainTypeId, ChainTypeId> typeMapping,
			IslandTypeId island
		) {
			ChainTypeId realType = typeMapping.Invoke(island, Info.ItemType);
			ChainInfo itemInfo = gameConfig.Chains[new LevelId<ChainTypeId>(realType, Info.ItemLevel)];
			if (Info.CostType == CurrencyTypeId.Gems) {
				return itemInfo.FlashSalePriceGems;
			}

			return itemInfo.FlashSalePrice;
		}
	}
}
