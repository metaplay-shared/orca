namespace Game.Logic {
	public class MergeInstruction {
		public Coordinates From { get; private set; }
		public Coordinates To { get; private set; }

		public MergeInstruction(Coordinates from, Coordinates to) {
			From = from;
			To = to;
		}
	}
}
