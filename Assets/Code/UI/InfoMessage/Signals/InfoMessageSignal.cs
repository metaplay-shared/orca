namespace Code.UI.InfoMessage.Signals {
	public class InfoMessageSignal {
		public string Message { get; }

		public InfoMessageSignal(object message) {
			Message = message.ToString();
		}
	}
}
