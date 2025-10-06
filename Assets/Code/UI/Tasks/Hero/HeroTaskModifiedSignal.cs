using Game.Logic;

namespace Code.UI.Tasks.Hero {
	public class HeroTaskModifiedSignal {
		public HeroTypeId HeroType { get; }

		public HeroTaskModifiedSignal(HeroTypeId heroType) {
			HeroType = heroType;
		}
	}
}
