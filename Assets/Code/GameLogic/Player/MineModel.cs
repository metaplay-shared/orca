using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class MineModel {
		[MetaMember(1)] public MineInfo Info { get; private set; }
		[MetaMember(2)] public int MineCycle { get; private set; }
		[MetaMember(3)] public int RepairCycle { get; private set; }
		[MetaMember(4)] public List<LevelId<ChainTypeId>> Queue { get; private set; }
		[MetaMember(5)] public MineState State { get; private set; }
		[MetaMember(6)] public int BuilderId { get; private set; }

		public MineModel() { }

		public MineModel(SharedGameConfig gameConfig, MineTypeId type) {
			Info = gameConfig.Mines[new LevelId<MineTypeId>(type, 1)];
			Queue = new List<LevelId<ChainTypeId>>();
			MineCycle = 0;
			RepairCycle = 0;
			State = MineState.Idle;
		}

		public MetaDuration RepairTime =>
			MetaDuration.Min(Info.MaxRepairTime, Info.BaseRepairTime + RepairCycle * Info.CycleRepairTime);

		public int EnergyUsage {
			get {
				if (Info.EnergyUsage.Count > 0) {
					int index = Math.Min(MineCycle, Info.EnergyUsage.Count - 1);
					return Info.EnergyUsage[index];
				}

				return 0;
			}
		}

		public void StartMining(int builderId) {
			BuilderId = builderId;
			State = MineState.Mining;
		}

		public void StartRepairing(int builderId) {
			BuilderId = builderId;
			State = MineState.Repairing;
		}

		public void UpdateState(SharedGameConfig gameConfig) {
			if (State == MineState.Mining) {
				BuilderId = 0;
				State = MineState.ItemsComplete;
			} else if (State == MineState.ItemsComplete) {
				MineCycle++;
				if (MineCycle >= Info.MineCycles) {
					if (Info.Disposable && RepairCycle + 1 >= Info.RepairCycles) {
						State = MineState.Destroyed;
					} else {
						State = MineState.NeedsRepair;
					}
				} else {
					State = MineState.Idle;
				}
			} else if (State == MineState.Repairing) {
				BuilderId = 0;
				State = MineState.Idle;
				MineCycle = 0;
				RepairCycle++;
				if (Info.RepairCycles > 0 && RepairCycle >= Info.RepairCycles) {
					RepairCycle = 0;
					LevelId<MineTypeId> nextLevelId = new LevelId<MineTypeId>(Info.Type, Info.Level + 1);
					if (gameConfig.Mines.ContainsKey(nextLevelId)) {
						Info = gameConfig.Mines[nextLevelId];
					}
				}
			}
		}

		public void CreateItems(RandomPCG random) {
			int index = Math.Min(MineCycle, Info.ItemsUsedOnCycle.Count - 1);
			int count = Info.ItemsUsedOnCycle[index];
			count = count >= 0 ? Math.Min(count, Info.Items.Count) : Info.Items.Count;

			for (int i = 0; i < count; i++) {
				Spawnable spawnable = Info.Items[i];
				CreateItem(random, spawnable);
			}

			if (MineCycle == Info.MineCycles - 1) {
				foreach (Spawnable spawnable in Info.LastCycleItems) {
					CreateItem(random, spawnable);
				}
			}
		}

		private void CreateItem(RandomPCG random, Spawnable spawnable) {
			F64 randomValue = F64.FromInt(random.NextInt(10000)) / 10000;
			if (randomValue > spawnable.DropRate) {
				return;
			}

			Queue.Add(new LevelId<ChainTypeId>(spawnable.Type, spawnable.Level));
		}
	}

	[MetaSerializable]
	public enum MineState {
		Idle,
		Mining,
		ItemsComplete,
		NeedsRepair,
		Repairing,
		Destroyed
	}
}
