using System.Collections.Generic;
using Code.UI.Application;
using Code.UI.Tutorial.TriggerActions;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using System;
using UnityEngine;
using Zenject;

namespace Code.UI.Tutorial {
	public class TriggerQueue {
		[Inject] private DiContainer container;
		[Inject] private ApplicationInfo applicationInfo;

		private readonly Queue<TriggerAction> actionQueue = new();

		private UniTaskCompletionSource taskCompletionSource;
		private bool running;

		private bool ShouldRunTrigger =>
			MetaplayClient.PlayerModel.GameConfig.Global.TriggersEnabled &&
			(applicationInfo.ActiveIsland.Value == null || applicationInfo.ActiveIsland.Value == IslandTypeId.None ||
				MetaplayClient.PlayerModel.GameConfig.Islands[applicationInfo.ActiveIsland.Value].TriggersEnabled);

		public void EnqueueAction(TriggerAction action) {
			if (ShouldRunTrigger || action is RewardTriggerAction || action is OfferTriggerAction) {
				actionQueue.Enqueue(action);
				RunQueue().Forget();
			}
		}

		private async UniTask RunQueue() {
			if (running) {
				return;
			}

			running = true;

			while (actionQueue.Count > 0) {
				TriggerAction action = actionQueue.Dequeue();
				container.Inject(action);
				try {
					await action.Run();
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}

			Debug.Log("Sequence completed");
			running = false;
		}
	}
}
