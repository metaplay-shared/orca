using Code.UI.ItemHolder.Signals;
using Zenject;

namespace Code.UI.ItemHolder {
	public class ItemHolderInstaller : Installer {
		public override void InstallBindings() {
			Container.DeclareSignal<ItemHolderModifiedSignal>().OptionalSubscriber();
		}
	}
}
