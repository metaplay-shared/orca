using System.Collections.Generic;

namespace Game.Logic {
	public interface ILevelInfo {
		int XpToNextLevel { get; }
		List<ResourceInfo> RewardResources { get; }
		List<ItemCountInfo> RewardItems { get; }
	}
}
