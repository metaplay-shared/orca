using Metaplay.Core.Model;

namespace Game.Logic {

	[ModelAction(ActionCodes.PlayerTerminateActivityEvent)]
	public class PlayerTerminateActivityEvent : PlayerAction {
		public EventId EventId { get; private set; }

		public PlayerTerminateActivityEvent() { }

		public PlayerTerminateActivityEvent(EventId eventId) {
			EventId = eventId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			ActivityEventModel activityEvent = player.ActivityEvents.TryGetState(EventId);
			if (activityEvent == null) {
				return ActionResult.NoSuchEvent;
			}

			if (activityEvent.Terminated) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				activityEvent.Terminate();
			}

			return ActionResult.Success;
		}
	}
}
