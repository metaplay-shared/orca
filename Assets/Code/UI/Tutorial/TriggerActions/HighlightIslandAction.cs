using Cysharp.Threading.Tasks;
using Game.Logic;
using Zenject;

namespace Code.UI.Tutorial.TriggerActions {
	public class HighlightIslandAction : TriggerAction {
		private readonly IslandTypeId islandTypeId;
		[Inject] private UIController uiController;

		public HighlightIslandAction(IslandTypeId islandTypeId) {
			this.islandTypeId = islandTypeId;
		}

		public override async UniTask Run() {
			await uiController.HighlightIsland(islandTypeId);
		}
	}
}
