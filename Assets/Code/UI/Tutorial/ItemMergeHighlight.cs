using System.Linq;
using Code.UI.MergeBase;
using Code.UI.MergeBase.Signals;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Code.UI.Tutorial {
	public class ItemMergeHighlight : Highlight {
		private readonly SignalBus signalBus;
		UniTaskCompletionSource tcs = new();

		public ItemMergeHighlight(SignalBus signalBus) {
			this.signalBus = signalBus;
		}

		protected override void PreProcess() {
			signalBus.Subscribe<ItemRemovedSignal>(OnItemRemoved);
		}

		protected override async UniTask Wait() {
			await tcs.Task;
		}

		void OnItemRemoved(ItemRemovedSignal signal) {
			bool highlightedItemRemoved = highlightedObjects
				.Where(o => o.GameObject.TryGetComponent<MergeItem>(out var _))
				.Select(o => o.GameObject.GetComponent<MergeItem>())
				.Any(i => i.LastKnownX == signal.X && i.LastKnownY == signal.Y);

			if (highlightedItemRemoved) {
				tcs.TrySetResult();
			}
		}

		protected override void PostProcess() {
			signalBus.Unsubscribe<ItemRemovedSignal>(OnItemRemoved);
		}
	}
}
