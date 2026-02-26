using JetBrains.Annotations;
using Zenject;

namespace Code.UI.Core.AndroidBackButton {
	[UsedImplicitly]
	public class AndroidBackButtonInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<AndroidBackButtonController>().AsSingle();
			Container.BindInterfacesTo<AndroidBackButtonInputController>().AsSingle();
		}
	}
}
