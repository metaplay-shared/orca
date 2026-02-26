using System.Collections.Generic;
using Metaplay.Core.Config;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {

	[MetaSerializable]
	public class TimerCostInfo : IGameConfigData<TimerTypeId> {
		[MetaMember(1)] public TimerTypeId Type { get; private set; }
		[MetaMember(2)] public CurrencyTypeId CurrencyType { get; private set; }
		[MetaMember(3)] public List<CostRangeInfo> Ranges { get; private set; }

		public int CalculateCost(int secondsLeft) {
			int totalCost = 0;
			int previousEndSeconds = 0;
			foreach (CostRangeInfo range in Ranges) {
				int rangeDuration = secondsLeft - previousEndSeconds;
				if (secondsLeft <= range.RangeEnd || range.RangeEnd == 0) {
					F64 rangeCost = F64.FromInt(rangeDuration) * range.CostPerUnit / 3600;
					totalCost += F64.CeilToInt(rangeCost);
					break;
				} else {
					totalCost += range.CostPerUnit * rangeDuration / 3600;
				}

				previousEndSeconds = range.RangeEnd;
			}

			return totalCost;
		}

		public TimerTypeId ConfigKey => Type;
	}
}
