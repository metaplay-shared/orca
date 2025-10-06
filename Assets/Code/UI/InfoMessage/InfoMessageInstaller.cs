using Code.UI.InfoMessage.Signals;
using Zenject;

namespace Code.UI.InfoMessage {
	public class InfoMessageInstaller : Installer {
		public override void InstallBindings() {
			Container.DeclareSignal<InfoMessageSignal>();
		}
	}
}
