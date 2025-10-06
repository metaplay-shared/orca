using System;
using System.Collections.Generic;
using System.Linq;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class RewardModel {
		[MetaMember(1)] public List<ResourceInfo> Resources { get; private set; }
		[MetaMember(2)] public List<ItemCountInfo> Items { get; private set; }
		[MetaMember(3)] public ChainTypeId ChestType { get; private set; }
		[MetaMember(4)] public int ChestLevel { get; private set; }
		[MetaMember(5)] public RewardMetadata Metadata { get; private set; }

		public RewardModel() {}

		public RewardModel(List<ResourceInfo> resources, List<ItemCountInfo> items, ChainTypeId chestType, int chestLevel, RewardMetadata metadata) {
			Resources = resources;
			Items = items;
			ChestType = chestType;
			ChestLevel = chestLevel;
			Metadata = metadata;
		}

		public override string ToString() {
			return $"{GetType().Name}[resources: [{String.Join(",", Resources.AsEnumerable().Select(info => info.ToString()).ToArray())}], " +
				$"items: [{String.Join(",", Items.AsEnumerable().Select(item => item.ToString()).ToArray())}]]";
		}
	}

	[MetaSerializable]
	public class RewardMetadata {
		[MetaMember(1)] public RewardType Type { get; set; }
		[MetaMember(2)] public int Level { get; set; }
		[MetaMember(3)] public IslandTypeId Island { get; set; }
		[MetaMember(4)] public HeroTypeId Hero { get; set; }
		[MetaMember(5)] public ChainTypeId Item { get; set; }
		[MetaMember(6)] public EventId Event { get; set; }
		[MetaMember(7)] public InAppProductId Product { get; set; }
		[MetaMember(8)] public VipPassId VipPass { get; set; }
		[MetaMember(9)] public LogbookChapterId Chapter { get; set; }
	}

	[MetaSerializable]
	public enum RewardType {
		None,
		PlayerLevel,
		HeroLevel,
		IslandLevel,
		BuildingLevel,
		BuildingDaily,
		HeroUnlock,
		BuildingFragment,
		ActivityEventLevel,
		ActivityEventAutoClaim,
		DailyTaskAutoClaim,
		IslandTask,
		VipPassDaily,
		LogbookChapter
	}
}
