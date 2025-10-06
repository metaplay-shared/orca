using UnityEngine;
using Zenject;

namespace Code.UI.Rewarding {
	public class RewardInstaller : Installer {
		public override void InstallBindings() {
			Container.DeclareSignal<RewardClaimedSignal>().OptionalSubscriber();
			Container.DeclareSignal<RewardReceivedSignal>().OptionalSubscriber();
			Container.DeclareSignal<RewardShownSignal>().OptionalSubscriber();

			Container.Bind<RewardBehaviour>().FromMethod(CreateRewardBehaviour);
			Container.BindInterfacesTo<RewardController>().AsSingle();

			Container.BindSignal<RewardReceivedSignal>().ToMethod(OnRewardReceived);
		}

		private void OnRewardReceived(RewardReceivedSignal signal) {
			Container.Resolve<RewardBehaviour>().EnqueueReward();
		}

		private RewardBehaviour CreateRewardBehaviour() {
			GameObject go = new GameObject();
			var component = (RewardBehaviour)Container.InstantiateComponent(typeof(RewardBehaviour), go);

			return component;
		}
	}
}
