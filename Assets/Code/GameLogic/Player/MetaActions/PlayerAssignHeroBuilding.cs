using System.Collections.Generic;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerAssignHeroBuilding)]
	public class PlayerAssignHeroBuilding : PlayerAction {
		public HeroTypeId HeroType { get; private set; }
		public ChainTypeId Building { get; private set; }

		public PlayerAssignHeroBuilding() { }

		public PlayerAssignHeroBuilding(HeroTypeId heroType, ChainTypeId building) {
			HeroType = heroType;
			Building = building;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Heroes.Heroes.ContainsKey(HeroType)) {
				return ActionResult.InvalidParam;
			}

			if (!player.GameConfig.Chains.ContainsKey(new LevelId<ChainTypeId>(Building, 1))) {
				return ActionResult.InvalidParam;
			}

			HeroModel hero = player.Heroes.Heroes[HeroType];

			if (hero.CurrentTask != null && hero.CurrentTask.State != HeroTaskState.Created) {
				return ActionResult.InvalidState;
			}

			ChainTypeId sourceBuilding = hero.Building;
			List<HeroModel> heroesInSourceBuilding = player.Heroes.HeroesInBuilding(sourceBuilding);
			if (heroesInSourceBuilding.Count <= 1) {
				return ActionResult.TooFewHeroesInBuilding;
			}

			List<HeroModel> heroesInTargetBuilding = player.Heroes.HeroesInBuilding(Building);
			if (heroesInTargetBuilding.Count >= player.GameConfig.Global.MaxHeroesInBuilding) {
				return ActionResult.TooManyHeroesInBuilding;
			}

			AssignHeroCostInfo costInfo = player.Heroes.AssignHero.AssignHeroCost(player.GameConfig);
			if (player.Wallet.Currency(costInfo.CurrencyType).Value < costInfo.Cost) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				hero.AssignToBuilding(Building);
				hero.Update(player.GameConfig, player.Level.Level, player.Inventory.UnlockedResourceItems, player.CurrentTime, player.ClientListener);
				player.ConsumeResources(costInfo.CurrencyType, costInfo.Cost, ResourceModificationContext.Empty);
				player.Heroes.AssignHero.UpdateCurrentIndex(player.GameConfig);
				player.ClientListener.OnHeroMovedToBuilding(HeroType, sourceBuilding, Building);
				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.AssignHero,
						costInfo.CurrencyType,
						costInfo.Cost,
						"",
						0,
						CurrencyTypeId.Hero,
						0,
						HeroType.Value
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
