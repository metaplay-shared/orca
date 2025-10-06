using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerFulfillHeroTask)]
	public class PlayerFulfillHeroTask : PlayerAction {
		public HeroTypeId HeroType { get; private set; }

		public PlayerFulfillHeroTask() { }

		public PlayerFulfillHeroTask(HeroTypeId heroType) {
			HeroType = heroType;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Heroes.Heroes.ContainsKey(HeroType)) {
				return ActionResult.InvalidParam;
			}

			HeroModel hero = player.Heroes.Heroes[HeroType];
			if (hero.CurrentTask == null) {
				return ActionResult.InvalidState;
			}

			if (hero.CurrentTask.State != HeroTaskState.Created) {
				return ActionResult.InvalidState;
			}

			if (!player.Inventory.HasEnoughResources(hero.CurrentTask.Info)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				foreach (ResourceInfo resource in hero.CurrentTask.Info.Resources) {
					player.ConsumeResources(resource.Type, resource.Amount, ResourceModificationContext.Empty);
				}

				player.EventStream.Event(new PlayerHeroTaskFulfilled(HeroType, hero.CurrentTask.Info.Id));
				hero.FulfillTask(player.CurrentTime);
				player.ClientListener.OnHeroTaskModified(HeroType);
			}

			return ActionResult.Success;
		}
	}
}
