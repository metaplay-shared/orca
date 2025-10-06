using Metaplay.Core;
using Metaplay.Core.Activables;
using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerPurchaseActivityEventPremiumPass)]
	public class PlayerPurchaseActivityEventPremiumPass : PlayerAction {
		public EventId EventId { get; private set; }
		public PlayerPurchaseActivityEventPremiumPass() { }

		public PlayerPurchaseActivityEventPremiumPass(EventId eventId) {
			EventId = eventId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			player.ActivityEvents.SubEnsureHasState(player.GameConfig.ActivityEvents[EventId], player);
			ActivityEventModel activityEvent = player.ActivityEvents.TryGetState(EventId);
			if (activityEvent == null) {
				return ActionResult.NoSuchEvent;
			}
			if (activityEvent.HasPremiumPass()) {
				return ActionResult.Success;
			}

			MetaActivableVisibleStatus eventStatus = player.Status(activityEvent);
			if (eventStatus == null ||
				!(eventStatus is MetaActivableVisibleStatus.Active ||
					eventStatus is MetaActivableVisibleStatus.EndingSoon ||
					eventStatus is MetaActivableVisibleStatus.InReview)) {
				return ActionResult.InvalidState;
			}

			ResourceInfo cost = activityEvent.Info.PremiumPassPrice;
			if (!player.Wallet.EnoughCurrency(cost.Type, cost.Amount)) {
				return ActionResult.NotEnoughResources;
			}

			if (commit) {
				player.ConsumeResources(cost.Type, cost.Amount, ResourceModificationContext.Empty);
				activityEvent.PurchasePremiumPass(player.CurrentTime);
				player.ClientListener.OnActivityEventPremiumPassBought(EventId);
				player.EventStream.Event(
					new PlayerEconomyAction(
						player,
						EconomyActionId.ActivityEventPremiumPassPurchased,
						cost.Type,
						cost.Amount,
						"",
						0,
						CurrencyTypeId.None,
						0,
						$"{EventId.Value}:PremiumPass"
					)
				);
			}

			return ActionResult.Success;
		}
	}
}
