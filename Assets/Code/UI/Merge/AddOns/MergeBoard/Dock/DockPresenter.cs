using System.Linq;
using Code.UI.MergeBase;
using Game.Logic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeBoard.Dock {
	public class DockPresenter : MergeBoardAddOn {
		[SerializeField] private RectTransform DockContainer;
		[SerializeField] private Sprite DockImage;

		[Inject] private MergeBoardRoot mergeBoard;
		[Inject] private DiContainer container;

		protected override void Clear() {
			foreach (Transform child in DockContainer) {
				Destroy(child.gameObject);
			}
		}

		protected override void Show() {
			var dockTiles = GetTiles((tile, pattern) => pattern == TileType.ItemHolder);

			// The board does not have place for dock configured
			if (dockTiles.Count == 0) {
				return;
			}

			int minX = dockTiles.Select(t => t.Tile.X).Min();
			int minY = dockTiles.Select(t => t.Tile.Y).Min();

			var tile = dockTiles.First(t => t.Tile.X == minX && t.Tile.Y == minY);
			SpawnDockTile(tile);
		}

		private void SpawnDockTile(MergeTileInfo dockTile) {
			GameObject dockTileGo = new GameObject();
			DockFlightTarget dockFlightTarget = dockTileGo.AddComponent<DockFlightTarget>();
			container.Inject(dockFlightTarget);
			RectTransform dockTileRt = dockTileGo.AddComponent<RectTransform>();

			dockTileRt.SetParent(DockContainer, false);
			Image dockImage = dockTileGo.AddComponent<Image>();
			dockImage.sprite = DockImage;
			dockImage.raycastTarget = false;

			dockTileRt.sizeDelta = new Vector2(MergeBoardRoot.TILE_WIDTH * 2, MergeBoardRoot.TILE_HEIGHT * 1.5f);
			dockTileRt.pivot = new Vector2(0.25f, 0.75f);

			dockTileRt.position = dockTile.Tile.Handle.GetComponent<RectTransform>().position;
		}
	}
}
