using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSkipHeroTaskTimer)]
	public class PlayerSkipHeroTaskTimer : PlayerAction {
		public HeroTypeId HeroType { get; private set; }

		public PlayerSkipHeroTaskTimer() { }

		public PlayerSkipHeroTaskTimer(HeroTypeId heroType) {
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

			if (hero.CurrentTask.State != HeroTaskState.Fulfilled) {
				return ActionResult.InvalidState;
			}

			Cost cost = player.SkipHeroTaskTimerCost(HeroType);
			if (player.Wallet.Currency(cost.Type).Value < cost.Amount) {
				return ActionResult.NotEnoughResources;
			}

			int timeLeft = F64.RoundToInt((hero.CurrentTask.FinishedAt - player.CurrentTime).ToSecondsF64());

			if (commit) {
				hero.CurrentTask.FinishNow(player.CurrentTime);
				player.ClientListener.OnHeroTaskModified(HeroType);
				player.ConsumeResources(cost.Type, cost.Amount, ResourceModificationContext.Empty);
				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.HeroTaskTimerSkipped,
						cost.Type,
						cost.Amount,
						"",
						0,
						CurrencyTypeId.Time,
						timeLeft,
						HeroType.Value
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
