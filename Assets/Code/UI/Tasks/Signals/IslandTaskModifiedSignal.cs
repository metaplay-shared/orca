using Game.Logic;

namespace Code.UI.Tasks.Signals {
	public class IslandTaskModifiedSignal {
		public IslandTypeId Island { get; }
		public IslanderId Islander { get; }

		public IslandTaskModifiedSignal(IslandTypeId island, IslanderId islander) {
			Island = island;
			Islander = islander;
		}
	}
}
