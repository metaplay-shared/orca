using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ChainInfo : IGameConfigData<LevelId<ChainTypeId>> {
		[MetaMember(1)] public ChainTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public CategoryId Category { get; private set; }
		// TODO: LevelId<CreatorType>... vai LevelId<CreatorInfo>
		[MetaMember(4)] public CreatorTypeId CreatorType { get; private set; }
		[MetaMember(5)] public int CreatorLevel { get; private set; }
		[MetaMember(6)] public ConverterTypeId ConverterType { get; private set; }
		[MetaMember(7)] public int ConverterLevel { get; private set; }
		[MetaMember(8)] public bool Sellable { get; private set; }
		[MetaMember(9)] public bool Mergeable { get; private set; }
		[MetaMember(10)] public IslandTypeId TargetIsland { get; private set; }
		[MetaMember(11)] public List<ResourceInfo> DiscoveryRewards { get; private set; }
		[MetaMember(12)] public CurrencyTypeId CollectableType { get; private set; }
		[MetaMember(13)] public int CollectableValue { get; private set; }
		[MetaMember(14)] public int ConvertableValue { get; private set; }
		[MetaMember(15)] public MetaDuration OpenTime { get; private set; }
		[MetaMember(16)] public int SellPrice { get; private set; }
		[MetaMember(17)] public int BubblePrice { get; private set; }
		[MetaMember(18)] public int FlashSalePrice { get; private set; }
		[MetaMember(19)] public int FlashSalePriceGems { get; private set; }
		[MetaMember(20)] public F64 BubbleProbability { get; private set; }
		[MetaMember(21)] public List<Spawnable> OtherBubbleSpawn { get; private set; }
		[MetaMember(22)] public List<TriggerId> DiscoveredTriggers { get; private set; }
		[MetaMember(23)] public List<TriggerId> WaitingBuilderTriggers { get; private set; }
		[MetaMember(24)] public List<TriggerId> CollectedTriggers { get; private set; }
		[MetaMember(25)] public List<TriggerId> SelectedTriggers { get; private set; }
		[MetaMember(26)] public BoosterTypeId BoosterType { get; private set; }
		[MetaMember(27)] public bool Transferable { get; private set; }
		[MetaMember(28)] public int Width { get; private set; }
		[MetaMember(29)] public int Height { get; private set; }
		[MetaMember(30)] public List<ResourceInfo> CreateRewards { get; private set; }
		[MetaMember(31)] public SelectActionId SelectAction { get; private set; }
		[MetaMember(32)] public bool HeroTarget { get; private set; }
		[MetaMember(33)] public bool Building { get; private set; }
		[MetaMember(34)] public bool Movable { get; private set; }
		[MetaMember(35)] public MetaDuration BuildTime { get; private set; }
		[MetaMember(36)] public int MergeEventScore { get; private set; }
		[MetaMember(37)] public int BuildEventScore { get; private set; }
		[MetaMember(38)] public MineTypeId MineType { get; private set; }

		public LevelId<ChainTypeId> ConfigKey => new(Type, Level);
	}
}
