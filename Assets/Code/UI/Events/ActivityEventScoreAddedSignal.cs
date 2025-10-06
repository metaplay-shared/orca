using Game.Logic;

namespace Code.UI.Events {
	public class ActivityEventScoreAddedSignal {
		public EventId EventId { get; }
		public int Level { get; }
		public int DeltaScore { get; }

		public ActivityEventScoreAddedSignal(EventId eventId, int level, int deltaScore) {
			EventId = eventId;
			Level = level;
			DeltaScore = deltaScore;
		}
	}
}
