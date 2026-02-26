using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Unity.DefaultIntegration;
using UniRx;
using UnityEngine;

namespace Code.UI.Map.IslandOverlay {
	public interface IIslandOverlayViewModel {
		IReadOnlyReactiveProperty<bool> HasItemsToCollect { get; }
		IReadOnlyReactiveProperty<bool> HasBuildingDailyRewardAvailable { get; }
		IReadOnlyReactiveProperty<bool> HasItemsOnDock { get; }
		IReadOnlyReactiveProperty<bool> HasBuildingsPendingComplete { get; }
		IReadOnlyReactiveProperty<bool> HasMinesWithItemsComplete { get; }
		IReadOnlyReactiveProperty<bool> HasInteractableHeroTasks { get; }
		IReadOnlyReactiveProperty<bool> HasSomethingToDo { get; }
		// TODO: Refactor the viewModel to be event driven
		void Update();
	}

	[UsedImplicitly]
	public class IslandOverlayViewModel : IIslandOverlayViewModel {
		private readonly CompositeDisposable disposables = new();
		private readonly IReactiveProperty<bool> hasBuildingDailyRewardAvailable;
		private readonly IReactiveProperty<bool> hasBuildingsPendingComplete;
		private readonly IReactiveProperty<bool> hasItemsOnDock;
		private readonly IReactiveProperty<bool> hasItemsToCollect;
		private readonly IReactiveProperty<bool> hasMinesWithItemsComplete;
		private readonly IReactiveProperty<bool> hasInteractableHeroTasks;
		private readonly IReadOnlyReactiveProperty<bool> hasSomethingToDo;
		private readonly IslandModel islandModel;

		public IslandOverlayViewModel(
			IslandModel islandModel
		) {
			this.islandModel = islandModel;
			hasItemsToCollect = new ReactiveProperty<bool>(GetHasItemsToCollect());
			hasBuildingDailyRewardAvailable = new ReactiveProperty<bool>(GetHasBuildingDailyRewardAvailable());
			hasItemsOnDock = new ReactiveProperty<bool>(GetHasItemsOnDock());
			hasBuildingsPendingComplete = new ReactiveProperty<bool>(GetHasBuildingsPendingComplete());
			hasMinesWithItemsComplete = new ReactiveProperty<bool>(GetHasMinesWithItemsComplete());
			hasInteractableHeroTasks = new ReactiveProperty<bool>(GetHasInteractableHeroTasks());
			hasSomethingToDo = Observable.Merge(
				hasItemsToCollect,
				hasBuildingDailyRewardAvailable,
				hasItemsOnDock,
				hasBuildingsPendingComplete,
				hasMinesWithItemsComplete,
				hasInteractableHeroTasks
			).Select(
				_ =>
					hasItemsToCollect.Value ||
					hasBuildingDailyRewardAvailable.Value ||
					hasItemsOnDock.Value ||
					hasBuildingsPendingComplete.Value ||
					hasMinesWithItemsComplete.Value ||
					hasInteractableHeroTasks.Value
			).ToReadOnlyReactiveProperty();
		}

		IReadOnlyReactiveProperty<bool> IIslandOverlayViewModel.HasItemsToCollect => hasItemsToCollect;
		IReadOnlyReactiveProperty<bool> IIslandOverlayViewModel.HasBuildingDailyRewardAvailable =>
			hasBuildingDailyRewardAvailable;
		IReadOnlyReactiveProperty<bool> IIslandOverlayViewModel.HasItemsOnDock => hasItemsOnDock;
		IReadOnlyReactiveProperty<bool> IIslandOverlayViewModel.HasBuildingsPendingComplete =>
			hasBuildingsPendingComplete;
		IReadOnlyReactiveProperty<bool> IIslandOverlayViewModel.HasMinesWithItemsComplete => hasMinesWithItemsComplete;
		IReadOnlyReactiveProperty<bool> IIslandOverlayViewModel.HasInteractableHeroTasks => hasInteractableHeroTasks;
		IReadOnlyReactiveProperty<bool> IIslandOverlayViewModel.HasSomethingToDo => hasSomethingToDo;

		public void Update() {
			hasItemsToCollect.Value = GetHasItemsToCollect();
			hasBuildingDailyRewardAvailable.Value = GetHasBuildingDailyRewardAvailable();
			hasItemsOnDock.Value = GetHasItemsOnDock();
			hasBuildingsPendingComplete.Value = GetHasBuildingsPendingComplete();
			hasMinesWithItemsComplete.Value = GetHasMinesWithItemsComplete();
			hasInteractableHeroTasks.Value = GetHasInteractableHeroTasks();
		}

		private bool GetHasItemsToCollect() {
			return islandModel.HasItemsToCollect(MetaplayClient.PlayerModel.GameConfig);
		}

		private bool GetHasBuildingDailyRewardAvailable() {
			return islandModel.BuildingDailyRewardAvailable(
				MetaplayClient.PlayerModel.GameConfig,
				MetaplayClient.PlayerModel.CurrentTime
			);
		}

		private bool GetHasItemsOnDock() {
			return islandModel?.MergeBoard?.HasItemsOnDock() == true;
		}

		private bool GetHasBuildingsPendingComplete() {
			return islandModel?.MergeBoard?.BuildingsPendingComplete() ?? false;
		}

		private bool GetHasMinesWithItemsComplete() {
			return islandModel?.MergeBoard?.MinesWithItemsComplete() ?? false;
		}

		private bool GetHasInteractableHeroTasks() {
			if (islandModel?.Info.Type == IslandTypeId.MainIsland) {
				return MetaplayClient.PlayerModel.Heroes.HasInteractableHeroTasks(MetaplayClient.PlayerModel.Inventory);
			}

			return false;
		}
	}
}
