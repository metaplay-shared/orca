using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class IslandInfo : IGameConfigData<IslandTypeId> {
		[MetaMember(1)] public IslandTypeId Type { get; private set; }
		[MetaMember(2)] public int X { get; private set; }
		[MetaMember(3)] public int Y { get; private set; }
		[MetaMember(4)] public int BoardWidth { get; private set; }
		[MetaMember(5)] public int BoardHeight { get; private set; }
		[MetaMember(6)] public int PlayerLevel { get; private set; }
		[MetaMember(7)] public ResourceInfo UnlockCost { get; private set; }
		[MetaMember(8)] public BoardPatternInfo BoardPattern { get; private set; }
		[MetaMember(9)] public List<IslanderId> Islanders { get; private set; }
		[MetaMember(11)] public ChainTypeId IslandCreator { get; private set; }
		[MetaMember(12)] public ChainTypeId IslandChest { get; private set; }
		[MetaMember(13)] public bool TaskLooping { get; private set; }
		[MetaMember(14)] public string LockAreaPattern { get; private set; }
		[MetaMember(15)] public List<TriggerId> RevealTriggers { get; private set; }
		[MetaMember(16)] public List<TriggerId> UnlockTriggers { get; private set; }
		[MetaMember(17)] public List<TriggerId> RevealBuildingTriggers { get; private set; }
		[MetaMember(18)] public List<TriggerId> StartBuildingTriggers { get; private set; }
		[MetaMember(19)] public HeroTypeId Hero { get; private set; }
		[MetaMember(20)] public int HeroLevel { get; private set; }
		[MetaMember(21)] public bool TriggersEnabled { get; private set; }
		[MetaMember(22)] public List<TriggerId> EnterTriggers { get; private set; }
		[MetaMember(23)] public bool GenerateAllResources { get; private set; }

		public IslandTypeId ConfigKey => Type;
	}
}
