using System;
using System.Collections.Generic;
using Code.UI.MergeBase;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;
using MergeTileModel = Game.Logic.MergeTileModel;

namespace Code.UI.Merge.AddOns {
	public abstract class MergeBoardAddOn : MonoBehaviour {
		protected class MergeTileInfo {
			public MergeTile Tile;
			public TileType TileType;
		}

		[Inject] protected MergeBoardRoot MergeBoard;
		protected IslandTypeId IslandType { get; private set; }

		public void Setup(string islandTypeIdString) {
			Setup(IslandTypeId.FromString(islandTypeIdString));
		}

		public void Setup(IslandTypeId islandTypeId) {
			IslandType = islandTypeId;

			Clear();
			Show();
		}

		protected abstract void Clear();
		protected abstract void Show();

		protected List<MergeTileInfo> GetTiles(Func<MergeTileModel, TileType, bool> filter) {
			IslandModel islandModel = MetaplayClient.PlayerModel.Islands[IslandType];
			List<MergeTileInfo> selectedTiles = new();

			for (int y = 0; y < islandModel.MergeBoard.Info.BoardHeight; y++) {
				for (int x = 0; x < islandModel.MergeBoard.Info.BoardWidth; x++) {
					var pattern = islandModel.Info.BoardPattern[x, y];
					if (filter(islandModel.MergeBoard[x, y], pattern)) {
						selectedTiles.Add(
							new MergeTileInfo {
								Tile = MergeBoard.TileAt(x, y),
								TileType = pattern
							}
						);
					}
				}
			}

			return selectedTiles;
		}
	}
}
