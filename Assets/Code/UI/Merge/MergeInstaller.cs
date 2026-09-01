using Code.UI.Merge.AddOns.MergeBoard.LockArea;
using Zenject;

namespace Code.UI.Merge {
	public class MergeInstaller : Installer {
		public override void InstallBindings() {
			Container.DeclareSignal<BuildingChangedSignal>().OptionalSubscriber();
			Container.DeclareSignal<AreaStateChangedSignal>().OptionalSubscriber();
		}
	}
}
