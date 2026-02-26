using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class MapTriggerInfo : IGameConfigData<TriggerId> {
		[MetaMember(1)] public TriggerId Trigger { get; private set; }
		[MetaMember(2)] public ResourceInfo Resource { get; private set; }
		[MetaMember(3)] public IslandTypeId UnlockedIsland { get; private set; }

		public TriggerId ConfigKey => Trigger;
	}
}
