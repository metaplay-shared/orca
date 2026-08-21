using JetBrains.Annotations;
using Zenject;

namespace Code.Logbook {
	[UsedImplicitly]
	public class LogbookInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<LogbookController>().AsSingle();
			Container.BindInterfacesTo<ItemDiscoveryController>().AsSingle();
			Container.BindInterfacesTo<LogbookTasksController>().AsSingle();
			Container.BindInterfacesTo<LogbookFlowController>().AsSingle();

			Container.BindInterfacesTo<FocusIslandTaskOperationProcessor>().AsSingle();
			Container.BindInterfacesTo<OpenDailyTasksTaskOperationProcessor>().AsSingle();
			Container.BindInterfacesTo<OpenIslandTaskOperationProcessor>().AsSingle();
			Container.BindInterfacesTo<SelectItemTaskOperationProcessor>().AsSingle();
		}
	}
}
