using Game.Logic;
using UniRx;

namespace Code.UI.Application {
	public class ApplicationInfo {
		public IReactiveProperty<IslandTypeId> ActiveIsland { get; } =
			new ReactiveProperty<IslandTypeId>(IslandTypeId.None);
	}
}
