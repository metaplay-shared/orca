using System;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ProducerModel {
		[MetaMember(1)] public int ProducedAtUpdate { get; private set; }
		[MetaMember(2)] public MetaTime LastUpdated { get; private set; }

		public void Reset(MetaTime currentTime, int count = 0) {
			ProducedAtUpdate = count;
			LastUpdated = currentTime;
		}

		public void Consume(int count) {
			ProducedAtUpdate -= count;
		}

		public void Add(int count) {
			ProducedAtUpdate += count;
		}

		public MetaDuration TimeToNext(MetaTime currentTime, F64 productionPerHour, int max) {
			if (ProducedAtUpdate >= max) {
				return MetaDuration.Zero;
			}

			MetaDuration diff = currentTime - LastUpdated;
			F64 secondsToProduce = 3600 / productionPerHour;
			F64 diffSeconds = diff.ToSecondsF64();
			if (diffSeconds >= secondsToProduce) {
				return MetaDuration.Zero;
			}

			return MetaDuration.FromSeconds(secondsToProduce - diffSeconds, MetaDuration.RoundingMode.Ceil);
		}

		public int Update(MetaTime currentTime, F64 productionPerHour, int max) {
			if (currentTime <= LastUpdated) {
				return 0;
			}
			if (ProducedAtUpdate >= max) {
				LastUpdated = currentTime;
				return 0;
			}

			int oldValue = ProducedAtUpdate;
			MetaDuration diff = currentTime - LastUpdated;
			int produced = F64.FloorToInt(productionPerHour * diff.ToSecondsF64() / 3600);
			// This needs to happen even if produced == 0 to move LastUpdated forward when the producer is full.
			if (ProducedAtUpdate + produced >= max) {
				ProducedAtUpdate = max;
				LastUpdated = currentTime;
			} else if (produced > 0) {
				ProducedAtUpdate += produced;
				LastUpdated +=
					MetaDuration.FromSeconds(F64.FloorToInt(F64.FromInt(produced * 3600) / productionPerHour));
			}
			return ProducedAtUpdate - oldValue;
		}

		public MetaDuration TimeToFill(F64 productionPerHour, int maxSize, MetaTime currentTime) {
			int spaceLeft = maxSize - ProducedAtUpdate;
			if (spaceLeft > 0) {
				F64 secondsLeft = spaceLeft * 3600 / productionPerHour;
				secondsLeft -= (currentTime - LastUpdated).ToSecondsF64();
				return MetaDuration.FromSeconds(secondsLeft, MetaDuration.RoundingMode.Ceil);
			}

			return MetaDuration.Zero;
		}
	}
}
