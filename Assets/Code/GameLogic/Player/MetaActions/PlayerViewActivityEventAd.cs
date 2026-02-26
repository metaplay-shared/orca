using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerViewActivityEventAd)]
	public class PlayerViewActivityEventAd : PlayerAction {
		public EventId EventId { get; private set; }

		public PlayerViewActivityEventAd() { }

		public PlayerViewActivityEventAd(EventId eventId) {
			EventId = eventId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			IEventModel eventModel = player.TryGetEventModel(EventId);
			if (eventModel == null) {
				return ActionResult.NoSuchEvent;
			}

			if (commit) {
				eventModel.MarkAdSeen(player.CurrentTime);
				player.EventStream.Event(
					new PlayerEventAdSeen(EventId.Value)
				);
			}

			return MetaActionResult.Success;
		}
	}
}
