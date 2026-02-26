using Zenject;

namespace Code.ATT {
	public class ATTInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<EmptyATTController>().AsSingle();
		}
	}
}
