using Game.Logic;

namespace Code.UI.Tasks.Hero {
	public class HeroUnlockedSignal {
		public HeroTypeId HeroType { get; }

		public HeroUnlockedSignal(HeroTypeId heroType) {
			HeroType = heroType;
		}
	}
}
