namespace Code.UI.Application {
	/// <summary>
	/// Represents the state of the application.
	/// </summary>
	public enum ApplicationState {
		/// <summary>
		/// Application is being started.
		/// </summary>
		AppStart,

		/// <summary>
		/// Connecting to the server. A real application would also load its assets here.
		/// </summary>
		Initializing,

		/// <summary>
		/// Session with server has been established and we're playing the game.
		/// </summary>
		Game,
	}
}
