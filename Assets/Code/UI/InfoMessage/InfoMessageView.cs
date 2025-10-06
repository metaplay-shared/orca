using System.Collections.Generic;
using System.Threading.Tasks;
using Code.UI.InfoMessage.Signals;
using Cysharp.Threading.Tasks;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.InfoMessage {
	public class InfoMessageView : MonoBehaviour {
		[SerializeField] private GameObject MessageContainer;
		[SerializeField] private TMP_Text MessageText;

		[Inject] private SignalBus signalBus;

		private readonly Queue<string> messageQueue = new();
		private bool isMessageQueueRunning;
		private int messageDelayMs = 2000;

		private void Start() {
			signalBus.Subscribe<InfoMessageSignal>(OnInfoMessage);
			messageDelayMs = MetaplayClient.PlayerModel.GameConfig.Client.InfoMessageDelayMs;
		}

		private void OnInfoMessage(InfoMessageSignal signal) {
			// No duplicates allowed
			if (messageQueue.Contains(signal.Message)) {
				return;
			}

			messageQueue.Enqueue(signal.Message);

			RunQueueAsync().Forget();
		}

		private async UniTask RunQueueAsync() {
			if (isMessageQueueRunning) {
				return;
			}

			isMessageQueueRunning = true;

			MessageContainer.SetActive(true);

			while (messageQueue.Count > 0) {
				await RunMessageAsync(messageQueue.Dequeue());
			}

			isMessageQueueRunning = false;
			MessageContainer.SetActive(false);
		}

		private async Task RunMessageAsync(string message) {
			MessageText.text = message;
			await UniTask.Delay(messageDelayMs);
		}
	}
}
