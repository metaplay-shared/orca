using Code.UI.Core;
using Code.UI.MergeBase.Signals;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading;
using Zenject;

namespace Code.UI {
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class ItemCreatedSignalHandler {
		private readonly SignalBus signalBus;
		private readonly Queue<ItemCreatedSignal> signals = new();
		private readonly IUIRootController uiRootController;

		private bool isFlushing;

		public ItemCreatedSignalHandler(
			IUIRootController uiRootController,
			SignalBus signalBus
		) {
			this.uiRootController = uiRootController;
			this.signalBus = signalBus;
		}

		public void Enqueue(ItemCreatedSignal signal) {
			signals.Enqueue(signal);
			if (!isFlushing) {
				FlushSignalsQueue().Forget();
			}

			async UniTask FlushSignalsQueue() {
				try {
					isFlushing = true;

					await UniTask.WaitWhile(
						() => uiRootController.IsAnyUIVisible(),
						PlayerLoopTiming.Update,
						CancellationToken.None
					);

					while (signals.Count > 0) {
						ItemCreatedSignal next = signals.Dequeue();
						signalBus.Fire(next);
					}

					signalBus.Fire(new ItemsOnBoardChangedSignal());
				} finally {
					isFlushing = false;
				}
			}
		}
	}
}
