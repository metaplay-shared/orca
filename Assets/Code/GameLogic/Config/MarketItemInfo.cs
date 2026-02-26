using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class MarketItemInfo : IGameConfigData<LevelId<ShopCategoryId>> {
		[MetaMember(1)] public ShopCategoryId Category { get; private set; }
		[MetaMember(2)] public int Index { get; private set; }
		[MetaMember(3)] public ChainTypeId ItemType { get; private set; }
		[MetaMember(4)] public int ItemLevel { get; private set; }
		[MetaMember(5)] public CurrencyTypeId CurrencyType { get; private set; }
		[MetaMember(6)] public int Count { get; private set; }
		[MetaMember(7)] public int Available { get; private set; }
		[MetaMember(8)] public CurrencyTypeId CostType { get; private set; }
		[MetaMember(9)] public int Cost { get; private set; }
		[MetaMember(10)] public string Icon { get; private set; }
		
		[MetaMember(11)]
		public MetaRef<PlayerSegmentInfo> Segment { get; private set; }

		public LevelId<ShopCategoryId> ConfigKey => new(Category, Index);
	}
}
