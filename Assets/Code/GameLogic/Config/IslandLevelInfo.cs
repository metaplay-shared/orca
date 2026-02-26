using System.Collections.Generic;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class IslandLevelInfo : IGameConfigData<LevelId<IslandTypeId>>, ILevelInfo {
		[MetaMember(1)] public IslandTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public int XpToNextLevel { get; private set; }
		[MetaMember(4)] public List<ResourceInfo> RewardResources { get; private set; }
		[MetaMember(5)] public List<ItemCountInfo> RewardItems { get; private set; }
		[MetaMember(6)] public List<TriggerId> Triggers { get; private set; }

		public LevelId<IslandTypeId> ConfigKey => new(Type, Level);
	}
}
