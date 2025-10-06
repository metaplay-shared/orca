using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class HeroTaskInfo : IGameConfigData<int> {
		[MetaMember(1)] public int Id { get; private set; }
		[MetaMember(2)] public ChainTypeId ItemType { get; private set; }
		[MetaMember(3)] public int PlayerLevel { get; private set; }
		[MetaMember(4)] public int HeroLevel { get; private set; }
		[MetaMember(5)] public ChainTypeId Building { get; private set; }
		[MetaMember(6)] public bool GoldenTask { get; private set; }
		[MetaMember(7)] public bool RunInSequence { get; private set; }
		[MetaMember(8)] public MetaDuration CompletionTime { get; private set; }
		[MetaMember(9)] public int HeroXp { get; private set; }
		[MetaMember(10)] public int PlayerXp { get; private set; }
		[MetaMember(11)] public int HeroTaskEventScore { get; private set; }
		[MetaMember(12)] public List<ResourceInfo> Resources { get; private set; }
		[MetaMember(13)] public List<ItemCountInfo> Rewards { get; private set; }
		[MetaMember(14)] public int MinPlayerLevel { get; private set; }
		[MetaMember(15)] public int MaxPlayerLevel { get; private set; }
		[MetaMember(16)] public int MinHeroLevel { get; private set; }
		[MetaMember(17)] public int MaxHeroLevel { get; private set; }

		public int ConfigKey => Id;
	}
}
