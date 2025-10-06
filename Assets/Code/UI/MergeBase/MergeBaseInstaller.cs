using Code.UI.MergeBase.Signals;
using UnityEngine;
using Zenject;

namespace Code.UI.MergeBase {
	public class MergeBaseInstaller : Installer {
		public override void InstallBindings() {
			Container.DeclareSignal<ItemCreatedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ItemMovedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ItemRemovedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ItemStateChangedSignal>().OptionalSubscriber();

			Container.DeclareSignal<MergeItemReceivedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ItemsOnBoardChangedSignal>().OptionalSubscriber();
			Container.DeclareSignal<AreaUnlockedSignal>().OptionalSubscriber();
			Container.DeclareSignal<NewMergeTaskSignal>().OptionalSubscriber();
			Container.DeclareSignal<MergeTaskSelectedSignal>().OptionalSubscriber();
			Container.DeclareSignal<MergeTaskCompletedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ItemCollectedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ItemSelectedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ItemMergedSignal>().OptionalSubscriber();
			Container.DeclareSignal<PointItemSignal>().OptionalSubscriber();
			Container.DeclareSignal<BuilderUsedSignal>().OptionalSubscriber();
			Container.DeclareSignal<MergeBoardStateChangedSignal>().OptionalSubscriber();
		}

		private T InjectGameObject<T>() where T : MonoBehaviour {
			var obj = Object.FindObjectOfType<T>(true);
			Container.Inject(obj);

			return obj;
		}
	}
}
