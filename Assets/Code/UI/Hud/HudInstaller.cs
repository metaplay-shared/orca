using Code.UI.Hud.Signals;
using Zenject;

namespace Code.UI.Hud {
	public class HudInstaller : Installer {
		public override void InstallBindings() {
			Container.DeclareSignal<PlayerLevelChangedSignal>().OptionalSubscriber();
			Container.DeclareSignal<PlayerXpChangedSignal>().OptionalSubscriber();
			Container.DeclareSignal<BuildersChangedSignal>().OptionalSubscriber();
			Container.DeclareSignal<VipPassChangedSignal>().OptionalSubscriber();
		}
	}
}
