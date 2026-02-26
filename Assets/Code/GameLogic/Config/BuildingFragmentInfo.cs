using System.Collections.Generic;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class BuildingFragmentInfo : IGameConfigData<LevelId<IslandTypeId>> {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public int Index { get; private set; }
		[MetaMember(3)] public ChainTypeId Type { get; private set; }
		[MetaMember(4)] public List<ResourceInfo> RewardResources { get; private set; }
		[MetaMember(5)] public List<ItemCountInfo> RewardItems { get; private set; }
		[MetaMember(6)] public List<TriggerId> Triggers { get; private set; }

		public LevelId<IslandTypeId> ConfigKey => new(Island, Index);
	}
}
