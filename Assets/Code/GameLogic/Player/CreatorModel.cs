using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class CreatorModel {
		[MetaMember(1)] public CreatorInfo Info { get; set; }
		[MetaMember(2)] public ProducerModel Producer { get; private set; }
		[MetaMember(3)] public List<LevelId<ChainTypeId>> ItemQueue { get; private set; }
		[MetaMember(4)] public CreatorState State { get; private set; }
		[MetaMember(5)] public bool LastItemsAdded { get; private set; }
		[MetaMember(6)] public int WavesLeft { get; private set; }

		private F64 totalRate;

		public CreatorModel() { }

		public CreatorModel(CreatorInfo info, MetaTime currentTime) {
			Info = info;
			Producer = new ProducerModel();
			Producer.Reset(currentTime, Info.ItemCount);
			State = CreatorState.Consuming;
			ItemQueue = new List<LevelId<ChainTypeId>>(info.FirstItems);
			WavesLeft = Info.WaveCount;
			Init();
		}

		[MetaOnDeserialized]
		public void Init() {
			totalRate = Info.TotalDropRate();
		}

		public int ItemCount {
			get {
				if (Info.PartialConsuming || State == CreatorState.Consuming) {
					return ItemQueue.Count + Producer.ProducedAtUpdate;
				} else {
					return 0;
				}
			}
		}
		public bool Update(MetaTime currentTime) {
			if (Info.FullRechargeTime > MetaDuration.Zero) {
				if (State == CreatorState.Filling) {
					Producer.Update(
						currentTime,
						F64.FromInt(Info.ItemCount) * 3600 / Info.FullRechargeTime.ToSecondsF64(),
						Info.ItemCount
					);
					if (Producer.ProducedAtUpdate == Info.ItemCount) {
						State = CreatorState.Consuming;
						return true;
					}
				} else if (Info.PartialProduction) {
					int delta = Producer.Update(
						currentTime,
						F64.FromInt(Info.ItemCount) * 3600 / Info.PartialRechargeTime.ToSecondsF64(),
						Info.ItemCount
					);
					if (delta > 0 && Producer.ProducedAtUpdate == Info.ItemCount) {
						return true;
					}
				} else {
					Producer.Reset(currentTime, Producer.ProducedAtUpdate);
				}
			}

			return false;
		}

		public ItemModel CreateItem(
			IslandTypeId island,
			RandomPCG random,
			SharedGameConfig gameConfig,
			MetaTime currentTime,
			Func<IslandTypeId, ChainTypeId, ChainTypeId> typeMapper
		) {
			if (ItemQueue.Count > 0) {
				LevelId<ChainTypeId> chainId = ItemQueue[0];
				ItemQueue.RemoveAt(0);
				return new ItemModel(typeMapper.Invoke(island, chainId.Type), chainId.Level, gameConfig, currentTime, true);
			}

			Producer.Consume(1);
			if (Producer.ProducedAtUpdate == 0) {
				State = CreatorState.Filling;
				Producer.Reset(currentTime);
			}

			if (Producer.ProducedAtUpdate == 0 && Info.Disposable && !LastItemsAdded && Info.LastItems.Count > 0) {
				ItemQueue.AddRange(Info.LastItems);
				LastItemsAdded = true;
			}

			F64 randomValue = F64.FromInt(random.NextInt(F64.RoundToInt(totalRate * 10000))) / 10000;
			F64 current = F64.Zero;
			foreach (Spawnable spawnable in Info.Spawnables) {
				current += spawnable.DropRate;
				if (randomValue < current) {
					return new ItemModel(typeMapper.Invoke(island, spawnable.Type), spawnable.Level, gameConfig, currentTime, true);
				}
			}

			return new ItemModel(
				typeMapper.Invoke(island, Info.Spawnables[0].Type),
				Info.Spawnables[0].Level,
				gameConfig,
				currentTime,
				true
			);
		}

		public MetaDuration TimeToFill(MetaTime currentTime) {
			return Producer.TimeToFill(
				F64.FromInt(Info.ItemCount) * 3600 / Info.FullRechargeTime.ToSecondsF64(),
				Info.ItemCount,
				currentTime
			);
		}

		public void UseWave() {
			WavesLeft--;
		}
	}

	[MetaSerializable]
	public enum CreatorState {
		Filling,
		Consuming
	}
}
