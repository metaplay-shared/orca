using Game.Logic;

namespace Code.UI.Events {
	public class ActivityEventPremiumPassBoughtSignal {
		public EventId EventId { get; }

		public ActivityEventPremiumPassBoughtSignal(EventId eventId) {
			EventId = eventId;
		}
	}
}
