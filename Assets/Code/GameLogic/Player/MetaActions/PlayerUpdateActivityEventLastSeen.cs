using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerUpdateActivityEventLastSeen)]
	public class PlayerUpdateActivityEventLastSeen : PlayerAction {
		public EventId EventId { get; private set; }

		public PlayerUpdateActivityEventLastSeen() { }

		public PlayerUpdateActivityEventLastSeen(EventId eventId) {
			EventId = eventId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			ActivityEventModel activityEvent = player.ActivityEvents.TryGetState(EventId);
			if (activityEvent == null) {
				return ActionResult.NoSuchEvent;
			}

			if (commit) {
				activityEvent.RefreshLastSeen();
			}
			return MetaActionResult.Success;
		}
	}
}
