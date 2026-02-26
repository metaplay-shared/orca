using Metaplay.Unity.ConnectionStates;
using Metaplay.Unity.DefaultIntegration;
using UniRx;
using UnityEngine;
using Zenject;

namespace Code.UI {
	public class UnhealthyConnectionIndicator : MonoBehaviour, IInitializable {
		private bool IsHealthyConnection() {
			bool isHealthy = MetaplayClient.Connection.State
					is Connected connectedState &&
				connectedState.IsHealthy;
			return isHealthy;
		}

		private void HandleHealthyConnectionChanged(bool isHealthy) {
			gameObject.SetActive(!isHealthy);
		}

		public void Initialize() {
			Observable.EveryUpdate()
				.Select(_ => IsHealthyConnection())
				.ToReactiveProperty()
				.Subscribe(HandleHealthyConnectionChanged)
				.AddTo(gameObject);
		}
	}
}
