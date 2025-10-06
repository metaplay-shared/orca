using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class CostRangeInfo {
		[MetaMember(1)] public int RangeEnd { get; private set; }
		// Note, unit can mean anything here. The interpretation depends on the usage of the class.
		[MetaMember(2)] public int CostPerUnit { get; private set; }

		public CostRangeInfo() {}

		public CostRangeInfo(int rangeEnd, int costPerUnit) {
			RangeEnd = rangeEnd;
			CostPerUnit = costPerUnit;
		}
	}
}
