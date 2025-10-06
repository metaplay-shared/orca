using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerAcknowledgeBuilding)]
	public class PlayerAcknowledgeBuilding : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public PlayerAcknowledgeBuilding() { }

		public PlayerAcknowledgeBuilding(IslandTypeId islandId, int x, int y) {
			IslandId = islandId;
			X = x;
			Y = y;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(IslandId)) {
				return ActionResult.InvalidParam;
			}

			if (player.Islands[IslandId].State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			IslandModel island = player.Islands[IslandId];
			MergeBoardModel mergeBoard = island.MergeBoard;
			if (X < 0 || X >= mergeBoard.Info.BoardWidth) {
				return ActionResult.InvalidCoordinates;
			}

			if (Y < 0 || Y >= mergeBoard.Info.BoardHeight) {
				return ActionResult.InvalidCoordinates;
			}

			ItemModel item = mergeBoard[X, Y].Item;
			if (item == null) {
				return ActionResult.InvalidCoordinates;
			}

			if (item.State != ItemState.Free || !mergeBoard.LockArea.IsFree(X, Y)) {
				return ActionResult.InvalidState;
			}

			if (item.BuildState != ItemBuildState.PendingComplete) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				item.AcknowledgeBuilding();
				player.HandleItemDiscovery(item);
				player.ClientListener.OnMergeItemStateChanged(IslandId, item);
				foreach (ResourceInfo resource in item.Info.CreateRewards) {
					player.EarnResources(resource.Type, resource.Amount, IslandId, new MergeBoardResourceContext(item.X, item.Y));
				}

				player.AddActivityEventScore(
					ActivityEventType.Build,
					item.Info.BuildEventScore,
					new MergeBoardResourceContext(X, Y)
				);
			}

			return ActionResult.Success;
		}
	}
}
