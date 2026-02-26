using Zenject;

namespace Code.UI.Events {
	public class EventsInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<EventsFlowController>().AsSingle();
			Container.DeclareSignal<ActivityEventPremiumPassBoughtSignal>().OptionalSubscriber();
			Container.DeclareSignal<EventStateChangedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ActivityEventRewardClaimedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ActivityEventScoreAddedSignal>().OptionalSubscriber();
		}
	}
}
