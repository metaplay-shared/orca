using Code.UI.AssetManagement;
using Code.UI.Merge.AddOns;
using Code.UI.MergeBase;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge {
	public class MergeBoardBordersPresenter : MergeBoardAddOn {
		[SerializeField] private RectTransform BorderContainer;

		[Inject] private MergeBoardRoot mergeBoard;
		[Inject] private AddressableManager addressableManager;

		protected override void Clear() {
			foreach (Transform child in BorderContainer) {
				Destroy(child.gameObject);
			}
		}

		protected override void Show() {
			InstantiateBorders();
		}

		private void InstantiateBorders() {
			var board = MetaplayClient.PlayerModel.Islands[IslandType].MergeBoard;

			for (int y = -1; y <= board.Info.BoardHeight; y++) {
				for (int x = -1; x <= board.Info.BoardWidth; x++) {
					int code = board.GetWaterTileCode(x, y);

					if (code <= 0) {
						continue;
					}

					SpawnBorderPieceAsync(x, y, code);
				}
			}
		}

		private void SpawnBorderPieceAsync(int x, int y, int code) {
			int index = code & 0x0F;

			int corners = (code & 0xF0) >> 4;
			bool bottomLeft = (corners & 0x1) > 0;
			bool bottomRight = ((corners >> 1) & 0x1) > 0;
			bool topLeft = ((corners >> 2) & 0x1) > 0;
			bool topRight = ((corners >> 3) & 0x1) > 0;

			MainBorder(x, y, index);
			EdgeBorder(x, y, bottomLeft, bottomRight, topLeft, topRight);
		}

		private void MainBorder(int x, int y, int index) {
			if (index == 0) {
				return;
			}

			GameObject tileGo = new GameObject($"{x}, {y} ({index})");

			RectTransform tileRt = tileGo.AddComponent<RectTransform>();
			tileRt.anchorMin = Vector2.zero;
			tileRt.anchorMax = Vector2.zero;
			tileRt.anchoredPosition = CalculatePosition(x, y);

			tileRt.sizeDelta = new Vector2(MergeBoardRoot.TILE_WIDTH, MergeBoardRoot.TILE_HEIGHT);

			tileRt.SetParent(BorderContainer, false);

			Image image = tileGo.AddComponent<Image>();

			image.sprite = addressableManager.Get<Sprite>($"MergeBorder/{index}.png");
			image.raycastTarget = false;
		}

		private void EdgeBorder(
			int x,
			int y,
			bool bottomLeft,
			bool bottomRight,
			bool topLeft,
			bool topRight
		) {
			if (!bottomLeft &&
				!bottomRight &&
				!topLeft &&
				!topRight) {
				return;
			}

			SpawnEdge(bottomLeft, 1);
			SpawnEdge(bottomRight, 2);
			SpawnEdge(topLeft, 3);
			SpawnEdge(topRight, 4);

			void SpawnEdge(bool shouldSpawn, int index) {
				if (!shouldSpawn) {
					return;
				}

				GameObject tileGo = new GameObject($"{x}, {y} ({index})");

				RectTransform tileRt = tileGo.AddComponent<RectTransform>();
				tileRt.anchorMin = Vector2.zero;
				tileRt.anchorMax = Vector2.zero;
				tileRt.anchoredPosition = CalculatePosition(x, y);

				tileRt.sizeDelta = new Vector2(MergeBoardRoot.TILE_WIDTH, MergeBoardRoot.TILE_HEIGHT);

				tileRt.SetParent(BorderContainer, false);

				Image image = tileGo.AddComponent<Image>();

				image.sprite = addressableManager.Get<Sprite>($"MergeBorder/c{index}.png");
			}
		}

		private Vector2 CalculatePosition(int x, int y) {
			return new Vector2(
				x * MergeBoardRoot.TILE_WIDTH + MergeBoardRoot.TILE_WIDTH / 2,
				y * MergeBoardRoot.TILE_HEIGHT + MergeBoardRoot.TILE_HEIGHT / 2
			);
		}
	}
}
