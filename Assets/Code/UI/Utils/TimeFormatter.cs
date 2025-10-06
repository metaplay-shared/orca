namespace Code.UI.Utils {
	public static class TimeFormatter {
		public static string SecondsToHuman(this int seconds) {
			int displayHours = seconds / 3600;
			int displayMinutes = seconds / 60 % 60;
			int displaySeconds = seconds % 60;

			if (displayHours > 0) {
				return $"{displayHours} h {displayMinutes} m";
			} else if (displayMinutes > 0) {
				return $"{displayMinutes} m {displaySeconds} s";
			} else {
				return $"{displaySeconds} s";
			}
		}
	}
}
