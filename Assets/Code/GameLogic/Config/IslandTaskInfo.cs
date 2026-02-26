using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class IslandTaskInfo : IGameConfigData<LevelId<IslanderId>> {
		[MetaMember(1)] public IslanderId Islander { get; private set; }
		[MetaMember(2)] public int Id { get; private set; }
		[MetaMember(3)] public IslandTypeId Island { get; private set; }
		[MetaMember(4)] public List<LevelId<IslanderId>> Dependencies { get; private set; }
		[MetaMember(5)] public int PlayerXp { get; private set; }
		[MetaMember(6)] public int IslandXp { get; private set; }
		[MetaMember(7)] public List<ItemCountInfo> Items { get; private set; }
		[MetaMember(8)] public List<ResourceInfo> RewardResources { get; private set; }
		[MetaMember(9)] public List<ItemCountInfo> RewardItems { get; private set; }
		[MetaMember(10)] public List<TriggerId> Triggers { get; private set; }
		[MetaMember(11)] public int GroupId { get; private set; }
		[MetaMember(12)] public List<TriggerId> ItemsAvailableTriggers { get; private set; }

		public LevelId<IslanderId> ConfigKey => new(Islander, Id);
	}
}
