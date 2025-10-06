using Metaplay.Core;
using Metaplay.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace Code.UI.Application {
	public class ApplicationSceneManager {
		private readonly LogChannel logChannel = MetaplaySDK.Logs.CreateChannel("Scene manager");
		private ApplicationState currentState;

		public async Task SwitchGameState(ApplicationState newState) {
			currentState = newState;
			logChannel.Info($"State changed to {newState}");

			switch (currentState) {
				case ApplicationState.AppStart:
					await LoadSceneAsync("Start");
					break;
				case ApplicationState.Initializing:
					logChannel.Info("Loading Loading Scene");
					await LoadSceneAsync("Loading");
					break;

				case ApplicationState.Game:
					logChannel.Info("Loading Game Scene");
					await LoadSceneAsync("Game");
					break;

				default:
					Debug.LogError("Undefined game state!");
					break;
			}
		}

		private async Task LoadSceneAsync(string sceneName) {
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
			if (asyncOperation != null) {
				asyncOperation.allowSceneActivation = true;
				asyncOperation.completed += _ => {
					logChannel.Info($"{sceneName} loaded");
				};

				while (!asyncOperation.isDone) {
					logChannel.Info($"Loading {sceneName}...{asyncOperation.progress}");
					await Task.Yield();
				}

				logChannel.Info($"{sceneName} loaded");
			}
		}
	}
}
