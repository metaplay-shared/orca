using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class HeroInfo : IGameConfigData<HeroTypeId> {
		[MetaMember(1)] public HeroTypeId Type { get; private set; }
		[MetaMember(2)] public int Index { get; private set; }
		[MetaMember(3)] public ChainTypeId ItemType { get; private set; }
		[MetaMember(4)] public MetaDuration TaskCooldown { get; private set; }
		[MetaMember(5)] public ChainTypeId DefaultBuilding { get; private set; }

		public HeroTypeId ConfigKey => Type;
	}
}
