using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI {
	/// <summary>
	/// Represents the in-game application logic. Only gets spawned after a session has been
	/// established with the server, so we can assume all the state has been setup already.
	/// </summary>
	public class GameManager : MonoBehaviour {
		public Text NumClicksText; // Text to display the number of clicks so far.
		public Button ClickMeButton; // 'Click Me' button that invokes OnClickButton().
		public Text UnhealthyConnectionIndicator; // Indicator to display when connection is in an unhealthy state.

		void Update() {
			// Update the number of clicks on UI.
			NumClicksText.text = "0";

			// Show the unhealthy connection indicator.
			bool connectionIsUnhealthy =
				MetaplayClient.Connection.State is Metaplay.Unity.ConnectionStates.Connected connectedState &&
				!connectedState.IsHealthy;
			UnhealthyConnectionIndicator.gameObject.SetActive(connectionIsUnhealthy);
		}

		public void OnClickButton() { }
	}
}
