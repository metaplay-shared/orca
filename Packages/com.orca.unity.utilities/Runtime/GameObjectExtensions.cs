using UnityEngine;

namespace Orca.Unity.Utilities {
	public static class GameObjectExtensions {
		public static TComponent GetOrAddComponent<TComponent>(this GameObject gameObject)
			where TComponent : Component {
			if (!gameObject.TryGetComponent<TComponent>(out var component)) {
				component = gameObject.AddComponent<TComponent>();
			}

			return component;
		}

		public static TComponent GetOrAddComponent<TComponent>(this Component component) where TComponent : Component {
			return component.gameObject.GetOrAddComponent<TComponent>();
		}
	}
}
