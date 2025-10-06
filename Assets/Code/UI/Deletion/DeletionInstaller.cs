using JetBrains.Annotations;
using Zenject;

namespace Code.UI.Deletion {
	[UsedImplicitly]
	public class DeletionInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<DeletionController>().AsSingle();
		}
	}
}
