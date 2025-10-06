using Metaplay.Core.Config;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class MergeInfo : GameConfigKeyValue<MergeInfo> {
		[MetaMember(1)] public int MaxEnergy { get; private set; }
		[MetaMember(2)] public F64 GeneratedPerHour { get; private set; }
	}
}
