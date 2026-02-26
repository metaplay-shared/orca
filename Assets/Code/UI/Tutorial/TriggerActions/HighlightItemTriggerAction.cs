using Cysharp.Threading.Tasks;
using Game.Logic;
using Zenject;

namespace Code.UI.Tutorial.TriggerActions {
	public class HighlightItemTriggerAction : TriggerAction {
		[Inject] private Blackout blackout;
		private ItemModel model;

		public HighlightItemTriggerAction(ItemModel itemModel) {
			model = itemModel;
		}

		public override async UniTask Run() {
			await blackout.HighlightItem(model);
		}
	}
}
