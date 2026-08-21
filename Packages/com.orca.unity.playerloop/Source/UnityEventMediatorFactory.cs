using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Orca.Unity.PlayerLoop {
	[UsedImplicitly]
	public class UnityEventMediatorFactory : IFactory<UnityEventMediator> {
		public UnityEventMediator Create() {
			// Create hosting GameObject
			var gameObject = new GameObject("UnityEventMediator");
			Object.DontDestroyOnLoad(gameObject);
			gameObject.hideFlags |= HideFlags.HideInHierarchy;

			// Add the UnityEventMediator to host
			var mediator = gameObject.AddComponent<UnityEventMediator>();
			return mediator;
		}
	}
}
