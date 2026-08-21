using System.Linq;
using Code.UI.MergeBase;
using Code.UI.MergeBase.Signals;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Code.UI.Tutorial {
	public class ItemHighlight : Highlight {
		private readonly SignalBus signalBus;
		UniTaskCompletionSource tcs = new();

		public ItemHighlight(SignalBus signalBus) {
			this.signalBus = signalBus;
		}

		protected override void PreProcess() {
			signalBus.Subscribe<ItemCollectedSignal>(OnItemCollected);
		}

		protected override async UniTask Wait() {
			await tcs.Task;
		}

		void OnItemCollected(ItemCollectedSignal signal) {
			var adapter = highlightedObjects.Last().GameObject.GetComponent<MergeItem>().Adapter;

			if (signal.X == adapter.X &&
				signal.Y == adapter.Y) {
				tcs.TrySetResult();
			}
		}

		protected override void PostProcess() {
			signalBus.Unsubscribe<ItemCollectedSignal>(OnItemCollected);
		}
	}
}
