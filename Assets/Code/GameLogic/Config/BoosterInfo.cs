using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class BoosterInfo : IGameConfigData<BoosterTypeId> {
		[MetaMember(1)] public BoosterTypeId Type { get; private set; }
		/// <summary>
		/// The maximum level of the item that a wildcard booster can be applied to.
		/// </summary>
		[MetaMember(2)] public int MaxWildcardLevel { get; private set; }
		/// <summary>
		/// The initial build time of a consumable builder booster.
		/// </summary>
		[MetaMember(3)] public MetaDuration InitialBuildTime { get; private set; }
		[MetaMember(4)] public LevelId<ChainTypeId> DestructionItem { get; private set; }

		public BoosterTypeId ConfigKey => Type;
	}
}
