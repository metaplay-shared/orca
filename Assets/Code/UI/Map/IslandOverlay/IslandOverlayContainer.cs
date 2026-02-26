using System.Collections.Generic;
using Code.UI.Tutorial;
using Game.Logic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Code.UI.Map.IslandOverlay {
	public class IslandOverlayContainer : MonoBehaviour {
		[SerializeField] private IslandOverlay TemplateIslandOverlay;
		[SerializeField] private RectTransform OverlayContainer;

		[Inject] private DiContainer container;

		private readonly List<IslandOverlay> addedIslands = new();
		private CompositeDisposable disposables = new();

		public void AddIsland(Island island) {
			var subContainer = container.CreateSubContainer();
			subContainer.Bind<IslandModel>().FromInstance(island.Model).AsSingle();
			subContainer.BindInterfacesTo<IslandOverlayViewModel>().AsSingle();
			IslandOverlay instance =
				subContainer.InstantiatePrefabForComponent<IslandOverlay>(TemplateIslandOverlay, OverlayContainer);
			instance.Setup(island);
			HighlightableElement highlightable = instance.gameObject.AddComponent<HighlightableElement>();
			highlightable.SetHighlightType($"Island_{island.Model.Info.Type.Value}");

			addedIslands.Add(instance);
		}

		public void Clear() {
			disposables.Dispose();
			disposables = new CompositeDisposable();
			foreach (IslandOverlay island in addedIslands) {
				Destroy(island.gameObject);
			}
		}
	}
}
