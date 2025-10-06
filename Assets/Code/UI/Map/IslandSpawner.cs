using Code.UI.Map.IslandOverlay;
using Code.UI.Map.Signals;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Code.UI.Map {
	public class IslandSpawner : MonoBehaviour {
		[SerializeField] private IslandOverlayContainer IslandOverlayContainer;
		[SerializeField] private Camera Camera;

		[Inject] private DiContainer container;
		[Inject] private SignalBus signalBus;

		private void Awake() {
			Camera = Camera ? Camera : Camera.main;
		}

		private void Start() {
			SpawnIslands();
			signalBus.Subscribe<IslandRemovedSignal>(OnIslandRemoved);
		}

		private void OnIslandRemoved(IslandRemovedSignal signal) {
			foreach (Transform child in transform) {
				Island island = child.GetComponent<Island>();
				if (island.Model.Info.Type == signal.Island) {
					Destroy(child.gameObject);
				}
			}
		}

		[ContextMenu("Spawn islands")]
		private void SpawnIslands() {
			Clear();

			foreach (IslandModel island in MetaplayClient.PlayerModel.Islands.Values) {
				SpawnIslandAsync(island).Forget();
			}
		}

		private async UniTask SpawnIslandAsync(IslandModel model) {
			GameObject islandPrefab =
				await Addressables.LoadAssetAsync<GameObject>($"Island/{model.Info.Type}.prefab").Task;
			Island island = container.InstantiatePrefab(islandPrefab).GetComponent<Island>();
			island.name = model.Info.Type.Value;
			Transform islandTransform = island.transform;
			islandTransform.SetParent(transform, true);
			islandTransform.position = new Vector3(model.Info.X, 0, model.Info.Y);
			island.Model = model;
			IslandOverlayContainer.AddIsland(island);
		}

		private void Clear() {
			IslandOverlayContainer.Clear();
			foreach (Transform child in transform) {
				Destroy(child.gameObject);
			}
		}
	}
}
