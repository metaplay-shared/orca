using Code.UI.Merge.Hero;
using Code.UI.Tasks.Hero;
using Code.UI.Tasks.Signals;
using Zenject;

namespace Code.UI.Tasks {
	public class TaskInstaller : Installer {
		public override void InstallBindings() {
			Container.DeclareSignal<HeroUnlockedSignal>().OptionalSubscriber();
			Container.DeclareSignal<HeroTaskModifiedSignal>().OptionalSubscriber();
			Container.DeclareSignal<HeroAssignedToBuildingSignal>().OptionalSubscriber();
			Container.DeclareSignal<IslandTaskModifiedSignal>().OptionalSubscriber();
		}
	}
}
