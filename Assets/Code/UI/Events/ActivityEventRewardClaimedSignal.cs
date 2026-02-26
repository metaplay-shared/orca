using Game.Logic;

namespace Code.UI.Events {
	public class ActivityEventRewardClaimedSignal {
		public EventId EventId { get; }
		public int Level { get; }
		public bool Premium { get; }

		public ActivityEventRewardClaimedSignal(EventId eventId, int level, bool premium) {
			EventId = eventId;
			Level = level;
			Premium = premium;
		}
	}
}
