using System.Collections.Generic;
using System.Linq;
using Code.UI.Tutorial;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeBoard.LockArea {
	public class LockedTile {
		public int X;
		public int Y;
		public LockAreaInfo LockAreaInfo;
	}

	public class LockAreaPresenter : MergeBoardAddOn {
		[SerializeField] private RectTransform areaContainer;
		[SerializeField] private LockArea TemplateLockArea;

		[Inject] private DiContainer container;

		protected override void Clear() {
			foreach (Transform child in areaContainer) {
				Destroy(child.gameObject);
			}
		}

		protected override void Show() {
			CreateAreas();
		}

		private void CreateAreas() {
			var lockedTiles = GetLockedTiles();
			var lockedAreas = lockedTiles
				.Where(tile => !IsOpen(tile.LockAreaInfo))
				.GroupBy(tile => tile.LockAreaInfo.Index)
				.Select(group => group.ToList())
				.ToList();

			foreach (var area in lockedAreas) {
				CreateArea(area);
			}
		}

		private bool IsOpen(LockAreaInfo info) {
			return MetaplayClient.PlayerModel.Islands[info.IslandId].MergeBoard.LockArea.Areas[info.AreaIndex] ==
				AreaState.Open;
		}

		private void CreateArea(List<LockedTile> area) {
			LockAreaInfo info = area.First().LockAreaInfo;
			GameObject areaGo = new GameObject($"Lock area {info.Index}");
			HighlightableElement highlightable = areaGo.AddComponent<HighlightableElement>();
			RectTransform areaRt = areaGo.AddComponent<RectTransform>();
			areaRt.SetParent(areaContainer, false);
			LockArea lockArea = container.InstantiatePrefab(TemplateLockArea, areaGo.transform).GetComponent<LockArea>();

			lockArea.Setup(area);
			highlightable.SetHighlightType(lockArea.HighlightType);
		}

		private List<LockedTile> GetLockedTiles() {
			IslandModel islandModel = MetaplayClient.PlayerModel.Islands[IslandType];
			List<LockedTile> lockedTiles = new();

			for (int y = islandModel.MergeBoard.Info.BoardHeight - 1; y >= 0 ; y--) {
				for (int x = 0; x < islandModel.MergeBoard.Info.BoardWidth; x++) {
					var pattern = islandModel.Info.BoardPattern[x, y];
					var lockInfo = LockInfoAt(x, y);

					if (pattern == TileType.Sea ||
						lockInfo == null) {
						continue;
					}

					lockedTiles.Add(
						new LockedTile() {
							X = x,
							Y = y,
							LockAreaInfo = lockInfo
						}
					);
				}
			}

			return lockedTiles;
		}

		private LockAreaInfo LockInfoAt(int x, int y) {
			var tile = MetaplayClient.PlayerModel.Islands[IslandType].MergeBoard.LockArea[x, y];
			if (tile == LockAreaModel.NO_AREA) {
				return default;
			}

			var lockAreaId = new LockAreaId(IslandType, tile.ToString());

			return MetaplayClient.PlayerModel.GameConfig.LockAreas[lockAreaId];
		}
	}
}
