using System.Threading;
using Code.UI.Map;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using Orca.Common;
using Zenject;

namespace Code.UI.Tutorial.TriggerActions {
	public class HighlightIslandAppearingAction : TriggerAction {
		[Inject] private IWorldMapBehaviour worldMapBehaviour;
		[Inject] private UIController uiController;

		public override async UniTask Run() {
			// TODO: Use proper cancellation token
			CancellationToken ct = CancellationToken.None;
			// TODO: Remove the business logic from the world map behaviour.
			// It is odd that the unity behaviour hold information about what to reveal next.
			Option<IslandModel> nextIsland;
			while ((nextIsland = worldMapBehaviour.GetNextIslandToReveal()).HasValue) {
				foreach (IslandModel island in nextIsland) {
					await uiController.HighlightIsland(island.Info.Type); // TODO: Add cancellation token
					await worldMapBehaviour.RevealNextPendingIsland(ct);
					MetaplayClient.PlayerContext.ExecuteAction(new PlayerRevealIsland(island.Info.Type));
				}
			}
		}
	}
}
