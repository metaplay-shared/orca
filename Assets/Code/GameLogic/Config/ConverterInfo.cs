using System.Collections.Generic;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ConverterInfo : IGameConfigData<LevelId<ConverterTypeId>> {
		[MetaMember(1)] public ConverterTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public List<ItemCountInfo> Items { get; private set; }

		public LevelId<ConverterTypeId> ConfigKey => new(Type, Level);
	}
}
