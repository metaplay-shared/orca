using Code.UI.Map.Signals;
using UnityEngine;
using Zenject;

namespace Code.UI.Map {
	public class MapInstaller : Installer {
		public override void InstallBindings() {
			Container.Bind<IWorldMapBehaviour>().FromMethod(InjectGameObject<WorldMapBehaviour>).AsSingle();
			Container.Bind<IslandSpawner>().FromMethod(InjectGameObject<IslandSpawner>).AsSingle();

			Container.DeclareSignal<MapControlsZoomedCloseToIslandSignal>().OptionalSubscriber();
			Container.DeclareSignal<IslandStateChangedSignal>().OptionalSubscriber();
			Container.DeclareSignal<IslandRemovedSignal>().OptionalSubscriber();
			Container.DeclareSignal<IslandFocusedSignal>().OptionalSubscriber();
			Container.DeclareSignal<IslandHighlightedSignal>().OptionalSubscriber();
			Container.DeclareSignal<IslandPointedSignal>().OptionalSubscriber();
			Container.DeclareSignal<EnteredMapSignal>().OptionalSubscriber();
		}

		private T InjectGameObject<T>() where T : MonoBehaviour {
			var obj = Object.FindObjectOfType<T>(true);
			Container.Inject(obj);

			return obj;
		}
	}
}
