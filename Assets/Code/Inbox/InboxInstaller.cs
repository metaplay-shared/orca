using JetBrains.Annotations;
using Zenject;

namespace Code.Inbox {
	[UsedImplicitly]
	public class InboxInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<InboxController>().AsSingle();
		}
	}
}
