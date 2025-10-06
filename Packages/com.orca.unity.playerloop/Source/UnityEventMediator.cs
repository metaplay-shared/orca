using System;
using UnityEngine;

namespace Orca.Unity.PlayerLoop {
	public interface IUpdateUnityEventMediator {
		event Action OnUpdate;
	}

	public interface IFocusChangedUnityEventMediator {
		event Action<bool> OnFocusChanged;
	}

	public interface IApplicationPauseChangedUnityEventMediator {
		event Action<bool> OnApplicationPauseChanged;
	}

	public interface IDestroyedUnityEventMediator {
		event Action OnDestroyed;
	}

	public interface IApplicationQuitInvokedUnityEventMediator {
		event Action OnApplicationQuitInvoked;
	}
	
	public interface IKeyDownInputUnityEventMediator {
		event Action OnKeyDown;
	}

	public class UnityEventMediator : MonoBehaviour,
		IUpdateUnityEventMediator,
		IFocusChangedUnityEventMediator,
		IApplicationPauseChangedUnityEventMediator,
		IDestroyedUnityEventMediator,
		IApplicationQuitInvokedUnityEventMediator {
		public event Action OnUpdate;
		public event Action<bool> OnFocusChanged;
		public event Action<bool> OnApplicationPauseChanged;
		public event Action OnDestroyed;
		public event Action OnApplicationQuitInvoked;

		protected void Awake() {
			Debug.Log("Created new UnityEventMediator");
		}

		protected void Update() {
			OnUpdate?.Invoke();
		}

		protected void OnDestroy() {
			OnDestroyed?.Invoke();
		}

		protected void OnApplicationFocus(bool hasFocus) {
			OnFocusChanged?.Invoke(hasFocus);
		}

		protected void OnApplicationPause(bool pauseStatus) {
			OnApplicationPauseChanged?.Invoke(pauseStatus);
		}

		protected void OnApplicationQuit() {
			OnApplicationQuitInvoked?.Invoke();
		}
	}
}
