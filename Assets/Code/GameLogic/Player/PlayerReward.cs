using System;
using Metaplay.Core.Model;
using Metaplay.Core.Rewards;

namespace Game.Logic {
	[MetaSerializableDerived(1)]
	public class RewardResource : MetaPlayerReward<PlayerModel> {
		[MetaMember(1)] public int Amount { get; private set; }
		[MetaMember(2)] public CurrencyTypeId CurrencyType { get; private set; } = CurrencyTypeId.Gold;

		public RewardResource() { }
		public RewardResource(int amount, CurrencyTypeId currencyType) {
			Amount = amount;
			CurrencyType = currencyType;
		}

		public override string ToString() {
			return $"{typeof(RewardResource)}, Amount {Amount}, CurrencyType {CurrencyType}";
		}

		public override void Consume(PlayerModel playerModel, IRewardSource source) {
			playerModel.EarnResources(CurrencyType, Amount, IslandTypeId.MainIsland, new MailResourceContext());
		}
	}

	[MetaSerializableDerived(2)]
	public class RewardItem : MetaPlayerReward<PlayerModel> {
		[MetaMember(1)] public int Amount { get; private set; }
		[MetaMember(2)] public ChainTypeId ItemType { get; private set; } = ChainTypeId.Gold;
		[MetaMember(3)] public int Level { get; private set; }

		public RewardItem() { }
		public RewardItem(int amount, ChainTypeId itemType, int level) {
			Amount = amount;
			ItemType = itemType;
			Level = level;
		}

		public override string ToString() {
			return $"{typeof(RewardResource)}, Amount {Amount}, ItemType {ItemType}, Level {Level}";
		}

		public override void Consume(PlayerModel playerModel, IRewardSource source) {
			for (int i = 0; i < Amount; i++) {
				ItemModel itemModel = new ItemModel(ItemType, Level, playerModel.GameConfig, playerModel.CurrentTime, true);
				playerModel.AddItemToHolder(IslandTypeId.MainIsland, itemModel);
			}
		}
	}
}
