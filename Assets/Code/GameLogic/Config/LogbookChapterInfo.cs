using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class LogbookChapterId : StringId<LogbookChapterId> { }

	[MetaSerializable]
	public class LogbookChapterInfo : IGameConfigData<LogbookChapterId> {
		[MetaMember(1)] public LogbookChapterId Id { get; private set; }
		[MetaMember(2)] public int Index { get; private set; }
		[MetaMember(3)] public List<ResourceInfo> RewardResources { get; private set; }
		[MetaMember(4)] public List<ItemCountInfo> RewardItems { get; private set; }
		[MetaMember(5)] public string RewardIcon { get; private set; }
		[MetaMember(6)] public List<TriggerId> Triggers { get; private set; }

		public LogbookChapterId ConfigKey => Id;
	}
}
