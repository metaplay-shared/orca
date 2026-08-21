using Game.Logic;

namespace Code.UI.Merge.Hero {
	public class HeroAssignedToBuildingSignal {
		public HeroTypeId Hero { get; }
		public ChainTypeId SourceBuilding { get; }
		public ChainTypeId TargetBuilding { get; }

		public HeroAssignedToBuildingSignal(HeroTypeId hero, ChainTypeId sourceBuilding, ChainTypeId targetBuilding) {
			Hero = hero;
			SourceBuilding = sourceBuilding;
			TargetBuilding = targetBuilding;
		}
	}
}
