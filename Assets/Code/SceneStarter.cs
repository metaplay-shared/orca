using System.Threading;
using Code.UI.Application;
using UnityEngine;
using Zenject;

namespace Code {
	public class SceneStarter : MonoBehaviour {
		[Inject] private readonly IApplicationStateManager applicationStateManager;
		public async void Start() {
			CancellationToken cancellationToken;
			await applicationStateManager.SwitchToState(
				ApplicationState.Initializing,
				cancellationToken
			);
		}
	}
}
