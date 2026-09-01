using JetBrains.Annotations;
using Zenject;

namespace Code.UI.Core.UIBlock {
	[UsedImplicitly]
	public class UIBlockInstaller : Installer {
		public override void InstallBindings() {
			Container.Bind<IUIBlockController>().To<UIBlockController>().AsSingle();
			Container.Bind<IUIBlockOverlayProvider>().To<UIBlockOverlayProvider>().AsSingle();
		}
	}
}
