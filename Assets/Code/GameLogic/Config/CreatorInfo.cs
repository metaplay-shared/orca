using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class CreatorInfo : IGameConfigData<LevelId<CreatorTypeId>> {
		[MetaMember(1)] public CreatorTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public MetaDuration FullRechargeTime { get; private set; }
		[MetaMember(4)] public MetaDuration PartialRechargeTime { get; private set; }
		[MetaMember(5)] public int ItemCount { get; private set; }
		[MetaMember(6)] public List<Spawnable> Spawnables { get; private set; }
		[MetaMember(7)] public bool AutoSpawn { get; private set; }
		[MetaMember(8)] public int EnergyUsage { get; private set; }
		[MetaMember(9)] public bool RootCreator { get; private set; }
		[MetaMember(10)] public bool PartialConsuming { get; private set; }
		[MetaMember(11)] public bool PartialProduction { get; private set; }
		[MetaMember(12)] public LevelId<ChainTypeId> DestructionItem { get; private set; }
		[MetaMember(13)] public List<LevelId<ChainTypeId>> FirstItems { get; private set; }
		[MetaMember(14)] public List<LevelId<ChainTypeId>> LastItems { get; private set; }
		[MetaMember(15)] public int WaveCount { get; private set; }

		public bool Disposable => (FullRechargeTime == MetaDuration.Zero || WaveCount > 0) && !RootCreator;

		public F64 TotalDropRate() {
			F64 total = F64.Zero;
			foreach (Spawnable spawnable in Spawnables) {
				total += spawnable.DropRate;
			}

			return total;
		}

		public LevelId<CreatorTypeId> ConfigKey => new(Type, Level);
	}

	[MetaSerializable]
	public class Spawnable {
		[MetaMember(1)] public ChainTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public F64 DropRate { get; private set; }

		public LevelId<ChainTypeId> ChainId => new(Type, Level);

		public Spawnable() {}

		public Spawnable(ChainTypeId type, int level, F64 dropRate) {
			Type = type;
			Level = level;
			DropRate = dropRate;
		}
	}
}
