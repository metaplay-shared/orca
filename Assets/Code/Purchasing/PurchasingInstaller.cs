using JetBrains.Annotations;
using Zenject;

namespace Code.Purchasing {
	[UsedImplicitly]
	public class PurchasingInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<PurchasingFlowController>().AsSingle();
		}
	}
}