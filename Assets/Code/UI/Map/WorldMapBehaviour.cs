using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Code.UI.Map.Signals;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using Orca.Common;
using UnityEngine;
using Zenject;

namespace Code.UI.Map {
	public interface IWorldMapBehaviour {
		public void SetWorldSize(Rect worldRect);
		void SetViewRectInWorld(Rect viewRect);
		Option<IslandModel> GetNextIslandToReveal();
		UniTask RevealNextPendingIsland(CancellationToken ct);
	}

	public class WorldMapBehaviour : MonoBehaviour, IWorldMapBehaviour {
		[Header("Configuration"), SerializeField] private float AdditionalWorldBorderSize = 50f;
		[Header("References"), SerializeField] private Transform Ocean;
		[SerializeField] private ParticleSystem Particles;
		[SerializeField] private Camera Camera;
		[SerializeField] private Transform CloudTileContainer;
		[SerializeField] private CloudTile CloudTilePrefab;
		[SerializeField] private Transform CloudCutOutContainer;
		[SerializeField] private SpriteRenderer CloudCutOutPrefab;
		[SerializeField] private float CutOutAnimationDuration = 1.0f;

		[Inject] private SignalBus signalBus;

		private readonly Dictionary<(int x, int y), CloudTile> activeCloudTiles = new();
		private readonly Stack<CloudTile> reusableCloudTiles = new();
		private readonly List<CloudTile> cloudTilesToRemove = new();
		private readonly Queue<IslandModel> islandRevealQueue = new();
		private readonly List<IslandModel> revealedIslands = new();

		protected void Start() {
			RevealOpenedIslands();
			QueueRevealableIslands();

			signalBus.Subscribe<IslandStateChangedSignal>(OnIslandStateChanged);
		}

		public void SetWorldSize(Rect worldRect) {
			float scaleX = worldRect.width + AdditionalWorldBorderSize;
			float scaleY = worldRect.height + AdditionalWorldBorderSize;
			Vector3 oceanScale = Ocean.localScale;
			Ocean.localScale = new Vector3(scaleX, scaleY, oceanScale.z);

			Vector3 oceanPosition = Ocean.position;
			Ocean.position = new Vector3(worldRect.center.x, oceanPosition.y, worldRect.center.y);
		}

		public void SetViewRectInWorld(Rect viewRect) {
			cloudTilesToRemove.Clear();
			cloudTilesToRemove.AddRange(activeCloudTiles.Values.Where(tile => !tile.WorldRect.Overlaps(viewRect)));
			foreach (CloudTile cloudTile in cloudTilesToRemove) {
				RemoveCloudTile(cloudTile);
			}

			Vector2 cloudTileSize = CloudTilePrefab.transform.localScale;
			// Adding and subtracting 1 to ensure a clouded boarder on the edges when scrolling
			Vector2Int startTileIndex = new(
				Mathf.FloorToInt(viewRect.min.x / cloudTileSize.x) - 1,
				Mathf.FloorToInt(viewRect.min.y / cloudTileSize.y) - 1);
			Vector2Int endTileIndex = new(
				Mathf.FloorToInt(viewRect.max.x / cloudTileSize.x) + 1,
				Mathf.FloorToInt(viewRect.max.y / cloudTileSize.y) + 1);
			for (int x = startTileIndex.x; x <= endTileIndex.x; x++) {
				for (int y = startTileIndex.y; y <= endTileIndex.y; y++) {
					if (!activeCloudTiles.ContainsKey((x, y))) {
						CreateCloudTile(x, y);
					}
				}
			}
		}

		public Option<IslandModel> GetNextIslandToReveal() {
			return islandRevealQueue.TryPeek(out IslandModel island) ? island : default;
		}

		public UniTask RevealNextPendingIsland(CancellationToken ct) {
			return RevealIsland(islandRevealQueue.Dequeue(), ct);
		}

		private void CreateCloudTile(int x, int y) {
			if (!reusableCloudTiles.TryPop(out CloudTile tile)) {
				tile = Instantiate(CloudTilePrefab, CloudTileContainer);
			} else {
				tile.gameObject.SetActive(true);
			}

			tile.SetGridPosition(x, y);
			activeCloudTiles.Add((x, y), tile);
		}

		private void RemoveCloudTile(CloudTile cloudTile) {
			if (activeCloudTiles.Remove((cloudTile.GridX, cloudTile.GridY), out CloudTile removedTile)) {
				removedTile.gameObject.SetActive(false);
				reusableCloudTiles.Push(removedTile);
			}
		}

		protected void Update() {
			Transform particlesTransform = Particles.transform;
			Vector3 particlesPosition = particlesTransform.position;
			Vector3 cameraPosition = Camera.transform.position;
			particlesPosition.x = cameraPosition.x;
			particlesPosition.z = cameraPosition.z;
			particlesTransform.position = particlesPosition;
		}

		private void OnIslandStateChanged(IslandStateChangedSignal signal) {
			IslandModel model = MetaplayClient.PlayerModel.Islands[signal.IslandTypeId];
			if (model.State != IslandState.Hidden &&
				!islandRevealQueue.Contains(model) &&
				!revealedIslands.Contains(model)) {
				islandRevealQueue.Enqueue(model);
			}
		}

		private void RevealOpenedIslands() {
			foreach (IslandModel island in MetaplayClient.PlayerModel.Islands.Values.Where(
				island =>
					island.State != IslandState.Hidden &&
					island.State != IslandState.Revealing
			)) {
				RevealIsland(island, this.GetCancellationTokenOnDestroy()).Forget();
			}
		}

		private void QueueRevealableIslands() {
			foreach (IslandModel island in MetaplayClient.PlayerModel.Islands.Values.Where(
				island => island.State == IslandState.Revealing
			)) {
				if (MetaplayClient.PlayerModel.GameConfig.Global.TriggersEnabled) {
					islandRevealQueue.Enqueue(island);
				} else {
					RevealIsland(island, this.GetCancellationTokenOnDestroy()).Forget();
					MetaplayClient.PlayerContext.ExecuteAction(new PlayerRevealIsland(island.Info.Type));
				}
			}
		}

		private UniTask RevealIsland(IslandModel island, CancellationToken ct) {
			SpriteRenderer cutOut = Instantiate(CloudCutOutPrefab, CloudCutOutContainer);
			Transform cutOutTransform = cutOut.transform;
			Vector3 cutOutPosition = cutOutTransform.position;
			cutOutPosition.x = island.Info.X;
			cutOutPosition.z = island.Info.Y;
			cutOutTransform.position = cutOutPosition;
			revealedIslands.Add(island);
			return cutOutTransform
				.DOScale(0, CutOutAnimationDuration)
				.From()
				.OnUpdate(
					() => {
						foreach (CloudTile cloudTile in activeCloudTiles.Values) {
							cloudTile.UpdateCutOutTexture();
						}
					}
				)
				.ToUniTask(cancellationToken: ct);
		}
	}
}
