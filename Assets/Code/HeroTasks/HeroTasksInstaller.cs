using JetBrains.Annotations;
using Zenject;

namespace Code.HeroTasks {
	[UsedImplicitly]
	public class HeroTasksInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<HeroTasksController>().AsSingle();
		}
	}
}
