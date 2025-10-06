using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerOpenLockArea)]
	public class PlayerOpenLockArea : PlayerAction {
		public IslandTypeId IslandId { get; private set; }
		public char AreaIndex { get; private set; }

		public PlayerOpenLockArea() { }

		public PlayerOpenLockArea(IslandTypeId islandId, char areaIndex) {
			IslandId = islandId;
			AreaIndex = areaIndex;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Islands.ContainsKey(IslandId)) {
				return ActionResult.InvalidParam;
			}

			IslandModel island = player.Islands[IslandId];

			if (island.State != IslandState.Open) {
				return ActionResult.InvalidState;
			}

			MergeBoardModel mergeBoard = island.MergeBoard;
			if (mergeBoard.LockArea.Areas.GetValueOrDefault(AreaIndex) != AreaState.Opening) {
				return ActionResult.InvalidState;
			}

			LockAreaInfo areaInfo = player.GameConfig.LockAreas[new LockAreaId(IslandId, AreaIndex.ToString())];
			if (!player.Wallet.EnoughCurrency(areaInfo.UnlockCost.Type, areaInfo.UnlockCost.Amount)) {
				return ActionResult.NotEnoughResources;
			}

			if (areaInfo.UnlockProduct != InAppProductId.FromString("None")) {
				return ActionResult.IapRequired;
			}

			if (commit) {
				player.ConsumeResources(
					areaInfo.UnlockCost.Type,
					areaInfo.UnlockCost.Amount,
					ResourceModificationContext.Empty
				);

				island.OpenLockArea(
					player.GameConfig,
					AreaIndex,
					player.HandleItemDiscovery,
					player.HandleBuildingState,
					player.ClientListener
				);
				island.RunIslandTaskTriggers(player.ExecuteTrigger);
				player.UnlockIslands(true);

				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.AreaUnlocked,
						areaInfo.UnlockCost.Type,
						areaInfo.UnlockCost.Amount,
						"",
						0,
						CurrencyTypeId.None,
						0,
						areaInfo.ConfigKey.ToString()
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
