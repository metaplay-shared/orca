using Metaplay.Core.Activables;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;

namespace Code.UI.Events.EventCards.Components {
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class EventCardStatus : MonoBehaviour {
		private void Start() {
			EventCard eventCard = GetComponentInParent<EventCard>();

			TMP_Text text = GetComponent<TextMeshProUGUI>();
			text.text = eventCard.EventModel.Status(MetaplayClient.PlayerModel) is MetaActivableVisibleStatus.Active
				? "On going"
				: "Starting soon";
		}
	}
}
