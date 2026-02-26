using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class MineInfo : IGameConfigData<LevelId<MineTypeId>> {
		[MetaMember(1)] public MineTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public int MineCycles { get; private set; }
		[MetaMember(4)] public int RepairCycles { get; private set; }
		[MetaMember(5)] public bool Disposable { get; private set; }
		[MetaMember(6)] public bool Chest { get; private set; }
		[MetaMember(7)] public bool AutoMine { get; private set; }
		[MetaMember(8)] public bool AutoRepair { get; private set; }
		[MetaMember(9)] public MetaDuration MiningTime { get; private set; }
		[MetaMember(10)] public MetaDuration LastCycleMiningTime { get; private set; }
		[MetaMember(11)] public MetaDuration BaseRepairTime { get; private set; }
		[MetaMember(12)] public MetaDuration CycleRepairTime { get; private set; }
		[MetaMember(13)] public MetaDuration MaxRepairTime { get; private set; }
		[MetaMember(14)] public bool RequiresBuilder { get; private set; }
		[MetaMember(15)] public List<int> EnergyUsage { get; private set; }
		[MetaMember(16)] public List<int> ItemsUsedOnCycle { get; private set; }
		[MetaMember(17)] public List<Spawnable> Items { get; private set; }
		[MetaMember(18)] public List<Spawnable> LastCycleItems { get; private set; }
		[MetaMember(19)] public LevelId<ChainTypeId> DestructionItem { get; private set; }
		[MetaMember(20)] public int ChestEventScore { get; private set; }

		public LevelId<MineTypeId> ConfigKey => new(Type, Level);
	}
}
