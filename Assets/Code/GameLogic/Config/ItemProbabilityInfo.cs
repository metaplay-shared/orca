using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ItemProbabilityInfof {
		[MetaMember(1)] public ChainTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public F64 Probability { get; private set; }
	}
}
