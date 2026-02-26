using System.Collections.Generic;
using Game.Logic;
using Metaplay.Core.Config;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {

	[MetaSerializable]
	public class LockAreaUnlockInfo {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public char LockAreaIndex { get; private set; }

		public LockAreaUnlockInfo() {}

		public LockAreaUnlockInfo(IslandTypeId island, char lockAreaIndex) {
			Island = island;
			LockAreaIndex = lockAreaIndex;
		}

		public override string ToString() {
			return $"{Island}:{LockAreaIndex}";
		}
	}

	[MetaSerializable]
	public class VipPassInfo : IGameConfigData<VipPassId> {
		[MetaMember(1)] public VipPassId Id { get; private set; }
		[MetaMember(2)] public int MaxEnergyBoost { get; private set; }
		[MetaMember(3)] public List<LockAreaUnlockInfo> LockAreaUnlocks  { get; private set; }
		[MetaMember(4)] public F64 EnergyProductionFactor { get; private set; }
		[MetaMember(5)] public F64 BuilderTimerFactor { get; private set; }
		[MetaMember(6)] public List<ResourceInfo> DailyRewardResources { get; private set; }
		[MetaMember(7)] public List<ItemCountInfo> DailyRewardItems { get; private set; }

		public VipPassId ConfigKey => Id;
	}
}
