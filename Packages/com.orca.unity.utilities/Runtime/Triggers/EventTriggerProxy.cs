using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Orca.Unity.Utilities.Triggers {
	public class PointerClickTriggerProxy : EventTriggerProxyBase {
		public readonly UnityEvent<PointerEventData> OnClick = new GenericUnityEvent<PointerEventData>();
		protected override EventTriggerType EventID => EventTriggerType.PointerClick;

		protected override void OnEnable() {
			base.OnEnable();
			OnTrigger.AddListener(HandleOnTrigger);
		}

		protected override void OnDisable() {
			base.OnDisable();
			OnTrigger.RemoveListener(HandleOnTrigger);
		}

		private void HandleOnTrigger(BaseEventData baseEvent) {
			OnClick?.Invoke((PointerEventData) baseEvent);
		}
	}

	[Serializable]
	internal class GenericUnityEvent<TArg> : UnityEvent<TArg> { }

	[RequireComponent(typeof(EventTrigger))]
	public abstract class EventTriggerProxyBase : MonoBehaviour {
		private EventTrigger.Entry eventTriggerEntry;
		private EventTrigger EventTrigger { get; set; }

		public UnityEvent<BaseEventData> OnTrigger => eventTriggerEntry.callback;

		protected abstract EventTriggerType EventID { get; }

		private void Awake() {
			EventTrigger = gameObject.GetOrAddComponent<EventTrigger>();
			eventTriggerEntry = new EventTrigger.Entry {eventID = EventID};
		}

		protected virtual void OnEnable() {
			EventTrigger.triggers.Add(eventTriggerEntry);
		}

		protected virtual void OnDisable() {
			EventTrigger.triggers.Remove(eventTriggerEntry);
		}
	}
}
