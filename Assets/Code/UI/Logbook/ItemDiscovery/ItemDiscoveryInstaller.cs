using Code.UI.ItemDiscovery.Signals;
using Zenject;

namespace Code.UI.ItemDiscovery {
	public class ItemDiscoveryInstaller : Installer {
		public override void InstallBindings() {
			Container.DeclareSignal<ItemDiscoveryStateChangedSignal>().OptionalSubscriber();
		}
	}
}
