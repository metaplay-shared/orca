using UnityEngine;

namespace Orca.Unity.Utilities.Triggers {
	public static class MonoBehaviourMessageTriggersExtensions {
		public static PointerClickTriggerProxy GetPointerClickTrigger(this GameObject gameObject) {
			return gameObject.GetOrAddComponent<PointerClickTriggerProxy>();
		}

		public static PointerClickTriggerProxy GetAsyncPointerClickTrigger(this Component component) {
			return component.gameObject.GetPointerClickTrigger();
		}
	}
}
