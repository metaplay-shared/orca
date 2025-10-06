using TMPro;
using UnityEngine;

namespace Code.UI.Player {
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class PlayerIdText : MonoBehaviour {
		private TextMeshProUGUI text;

		private void Start() {
			text = GetComponent<TextMeshProUGUI>();
		}

		void Update()
		{
			if (MetaplayClient.State != null &&
				MetaplayClient.PlayerModel != null) {
				string playerName = MetaplayClient.PlayerModel.PlayerName;
				string playerId = MetaplayClient.PlayerModel.PlayerId.ToString();
				text.richText = true;
				text.text = $"{playerName}\n({playerId})";
			}
		}
	}
}
