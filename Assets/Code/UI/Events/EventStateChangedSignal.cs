using Game.Logic;

namespace Code.UI.Events {
	public class EventStateChangedSignal {
		public EventId EventId { get; }

		public EventStateChangedSignal(EventId eventId) {
			EventId = eventId;
		}
	}
}
