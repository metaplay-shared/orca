using Zenject;

namespace Code.UI.Core {
	public class UIRootInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<UIRootController>().AsSingle();
			Container.BindInterfacesTo<UIRootProvider>().AsSingle();
		}
	}
}
