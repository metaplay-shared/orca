using System;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class InitialItemInfo : IGameConfigData<IslandCoordinate> {
		[MetaMember(1)] public IslandTypeId IslandId { get; private set; }
		[MetaMember(2)] public int X { get; private set; }
		[MetaMember(3)] public int Y { get; private set; }
		[MetaMember(4)] public ChainTypeId Type { get; private set; }
		[MetaMember(5)] public int Level { get; private set; }
		[MetaMember(6)] public bool Free { get; private set; }
		[MetaMember(7)] public bool SkipFreeForMerge { get; private set; }
		[MetaMember(8)] public bool InitAsBuilt { get; private set; }

		public IslandCoordinate ConfigKey => new IslandCoordinate(IslandId, X, Y);
	}
}
