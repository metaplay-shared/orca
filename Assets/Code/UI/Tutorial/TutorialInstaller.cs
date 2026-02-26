using System;
using Code.UI.Tutorial.TriggerActions;
using Zenject;

namespace Code.UI.Tutorial {
	public class TutorialInstaller : Installer {
		public override void InstallBindings() {
			Container.Bind<TriggerQueue>().AsSingle();
			
			Container.BindInterfacesAndSelfTo<ItemHighlight>().AsTransient();
			Container.BindInterfacesAndSelfTo<UiElementHighlight>().AsTransient();
			Container.BindInterfacesAndSelfTo<ItemMergeHighlight>().AsTransient();

			Container.DeclareSignal<MergeHintSignal>().OptionalSubscriber();

			Container.DeclareSignal<DialogueOpenSignal>().OptionalSubscriber();
		}
	}
}
