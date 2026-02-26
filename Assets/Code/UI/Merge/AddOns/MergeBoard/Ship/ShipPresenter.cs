using System.Collections.Generic;
using Code.UI.Application;
using Code.UI.MergeBase;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeBoard.Ship {
	public class ShipPresenter : MergeBoardAddOn {
		[SerializeField] private RectTransform ShipFromLeft;
		[SerializeField] private RectTransform ShipFromRight;

		[Inject] private MergeBoardRoot mergeBoard;

		protected override void Clear() {
			ShipFromLeft.gameObject.SetActive(false);
			ShipFromRight.gameObject.SetActive(false);
		}

		protected override void Show() {
			List<MergeTileInfo> tiles = GetTiles((tile, pattern) => pattern == TileType.Ship);
			if (tiles.Count < 2) {
				ShipFromLeft.gameObject.SetActive(false);
				ShipFromRight.gameObject.SetActive(false);
				return;
			}
			tiles.Sort((t1, t2) => t1.Tile.X.CompareTo(t2.Tile.X));

			MergeTile left = tiles[0].Tile;
			Vector3 leftPosition = left.transform.position;
			MergeTile right = tiles[1].Tile;
			Vector3 rightPosition = right.transform.position;
			Vector3 diff = rightPosition - leftPosition;

			if (mergeBoard.TileAt(left.X - 1, left.Y) == null) {
				ShipFromLeft.gameObject.SetActive(true);
				ShipFromLeft.position = leftPosition - diff;
				ShipFromLeft.DOMove(leftPosition, 1.5f).ToUniTask(TweenCancelBehaviour.Complete).Forget();
			} else {
				ShipFromRight.gameObject.SetActive(true);
				ShipFromRight.position = rightPosition;
				ShipFromRight.DOMove(leftPosition, 1.5f).ToUniTask(TweenCancelBehaviour.Complete).Forget();
			}
 		}
	}
}
