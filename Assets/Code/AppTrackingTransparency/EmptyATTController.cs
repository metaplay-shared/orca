using JetBrains.Annotations;
using UnityEngine;

namespace Code.ATT {
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class EmptyATTController : IATTController {
		public void RequestAuthorizationTracking() {
			#if UNITY_EDITOR
			Debug.LogWarning("Request Authorization Tracking Ignored on editor");
			#endif
		}
	}
}
