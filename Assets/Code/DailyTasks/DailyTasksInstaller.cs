using JetBrains.Annotations;
using Zenject;

namespace Code.DailyTasks {
	[UsedImplicitly]
	public class DailyTasksInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<DailyTasksController>().AsSingle();
		}
	}
}
