using System.Collections.Generic;
using System.Linq;
using Code.UI.MergeBase;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Merge {
	public class BoardStateAdapter : IBoardStateAdapter {
		public IMergeItemModelAdapter[] MergeItems => items.ToArray();
		public string Identifier => islandType.Value;

		private DiContainer container;
		
		private readonly List<IMergeItemModelAdapter> items;
		private readonly IslandTypeId islandType;

		public BoardStateAdapter(List<ItemModel> mergeBoardItems, IslandTypeId islandType, DiContainer container) {
			this.islandType = islandType;
			items = mergeBoardItems
				.Select(i => new MergeItemModelAdapter(i, islandType) as IMergeItemModelAdapter)
				.ToList();

			foreach (var mergeItemModelAdapter in items) {
				container.Inject(mergeItemModelAdapter);
			}
		}

		public bool CanMoveFrom(int x, int y) {
			return MetaplayClient.PlayerModel.Islands[islandType].MergeBoard.CanMoveFrom(x, y);
		}

		public bool CanMoveTo(int fromX, int fromY, int toX, int toY) {
			var island = MetaplayClient.PlayerModel.Islands[islandType];
			var item = island.MergeBoard[fromX, fromY].Item;

			var result = MetaplayClient.PlayerModel.Islands[islandType].MoveResult(
				MetaplayClient.PlayerModel.GameConfig,
				item,
				toX,
				toY
			);
			return result != MergeBoardModel.MoveResultType.Invalid;
		}

		public bool CanMergeTo(int fromX, int fromY, int toX, int toY) {
			var island = MetaplayClient.PlayerModel.Islands[islandType];
			var item = island.MergeBoard[fromX, fromY].Item;

			var result = MetaplayClient.PlayerModel.Islands[islandType].MoveResult(
				MetaplayClient.PlayerModel.GameConfig,
				item,
				toX,
				toY
			);
			return result == MergeBoardModel.MoveResultType.Merge;
		}

		public void MoveItem(int fromX, int fromY, int toX, int toY) {
			var island = MetaplayClient.PlayerModel.Islands[islandType];
			if (island.IsTargetBuilding(toX, toY)) {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerClaimBuildingFragment(islandType, fromX, fromY));
			} else {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerMoveItemOnBoard(islandType, fromX, fromY, toX, toY));
			}
		}
	}
}
