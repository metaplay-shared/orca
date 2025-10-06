using System.Collections.Generic;
using Code.UI.MergeBase.Signals;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Zenject;

namespace Code.UI.Tutorial.TriggerActions {
	public class PointItemsTriggerAction : TriggerAction {
		[Inject] private SignalBus signalBus;

		public List<ItemModel> Items { get; private set; }

		public PointItemsTriggerAction(List<ItemModel> items) {
			Items = items;
		}

		public override async UniTask Run() {
			await UniTask.DelayFrame(2);
			foreach (var item in Items) {
				signalBus.Fire(new PointItemSignal(item.X, item.Y));
			}
		}
	}
}
