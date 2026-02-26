using System.Collections.Generic;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ActivityEventLevelInfo : IGameConfigData<LevelId<EventId>>, ILevelInfo {
		[MetaMember(1)] public EventId EventId { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public int ScoreToNextLevel { get; private set; }
		[MetaMember(4)] public List<ResourceInfo> RewardResources { get; private set; }
		[MetaMember(5)] public List<ItemCountInfo> RewardItems { get; private set; }
		[MetaMember(6)] public ItemCountInfo FreeRewardItem { get; private set; }
		[MetaMember(7)] public ItemCountInfo PremiumRewardItem { get; private set; }
		[MetaMember(8)] public List<TriggerId> Triggers { get; private set; }

		public LevelId<EventId> ConfigKey => new(EventId, Level);
		public int XpToNextLevel => ScoreToNextLevel;

		public override string ToString() {
			return $"{GetType().Name} {EventId.Value}:{Level}:{ScoreToNextLevel}";
		}
	}
}
