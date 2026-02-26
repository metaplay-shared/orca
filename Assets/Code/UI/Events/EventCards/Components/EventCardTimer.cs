using System;
using UnityEngine;

namespace Code.UI.Events.EventCards.Components {
	[RequireComponent(typeof(EventTimer))]
	public class EventCardTimer : MonoBehaviour {
		private void Start() {
			EventCard eventCard = GetComponentInParent<EventCard>();
			EventTimer eventTimer = GetComponent<EventTimer>();

			eventTimer.Setup(eventCard.EventModel.EventInfo.ActivableParams);
		}
	}
}
