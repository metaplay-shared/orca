using Code.ATT;
using Code.Inbox;
using Code.Privacy;
using Code.UI.AssetManagement;
using System.Threading;
using Metaplay.Core.Client;
using UnityEngine;
using Zenject;

namespace Code.UI.Application {
	[CreateAssetMenu(fileName = "AppInstaller", menuName = "Installers/AppInstaller")]
	public class AppInstaller : ScriptableObjectInstaller<AppInstaller> {
		public override void InstallBindings() {
			Container.BindInterfacesTo<ApplicationStateManager>().AsSingle();
			Container.BindInterfacesTo<DefaultEnvironmentConfigProvider>().AsSingle();
			Container.BindInterfacesTo<MetaplayClientAdapter>().AsSingle();
			Container.BindInterfacesTo<PrivacyController>().AsSingle();

			// TODO: Add proper cancellation token to the container
			Container.BindInstance(CancellationToken.None).AsSingle();
			Container.Bind<AddressableManager>().AsSingle();
			Container.Install<InboxInstaller>();
			Container.Install<ATTInstaller>();
		}
	}
}
