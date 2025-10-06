using System.Linq;
using System.Threading;
using Code.UI.MergeBase.Signals;
using Code.UI.RequirementsDisplay;
using Code.UI.Tasks.Signals;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;

namespace Code.UI.Tasks.Islanders {
	public class IslanderTaskDetails : TaskDetails {
		private IslanderId islander;
		private IslandTypeId islandType;

		public void Setup(IslandTypeId islandTypeId, IslanderId islanderId) {
			islander = islanderId;
			islandType = islandTypeId;

			UpdateRequirementsAsync(default).Forget();
			CompleteTaskButton.Setup(islandType, islanderId);
			//ClaimButton.Setup(taskGiverModel.Info.Type);

			ClaimButton.gameObject.SetActive(false);
			IslandTaskModel task = MetaplayClient.PlayerModel.Islands[islandType].Tasks.Tasks[islander];
			CompleteTaskButton.gameObject.SetActive(CanComplete(task.Info));
		}

		private async UniTask UpdateRequirementsAsync(CancellationToken ct) {
			IslandTaskModel task = MetaplayClient.PlayerModel.Islands[islandType].Tasks.Tasks[islander];
			CompleteTaskButton.gameObject.SetActive(CanComplete(task.Info));

			var requirementItems = task.Info.Items.Select(
				r => new RequirementItemItem() {
					RequiredAmount = r.Count,
					TypeName = r.Type.Value,
					ItemLevel = r.Level
				}
			).ToArray();

			await ResourceRequirements.SetupAsync(requirementItems, islandType);
		}

		private bool CanComplete(IslandTaskInfo taskInfo) {
			return MetaplayClient.PlayerModel.Islands[islandType].MergeBoard.HasItemsForTask(taskInfo);
		}

		private void OnEnable() {
			signalBus.Subscribe<ItemCreatedSignal>(OnItemsChanged);
			signalBus.Subscribe<ItemRemovedSignal>(OnItemsChanged);
			signalBus.Subscribe<IslandTaskModifiedSignal>(OnIslandTaskModified);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<ItemCreatedSignal>(OnItemsChanged);
			signalBus.Unsubscribe<ItemRemovedSignal>(OnItemsChanged);
			signalBus.Unsubscribe<IslandTaskModifiedSignal>(OnIslandTaskModified);
		}

		private void OnItemsChanged() {
			UpdateRequirementsAsync(default).Forget();
		}

		private void OnIslandTaskModified(IslandTaskModifiedSignal signal) {
			UpdateRequirementsAsync(default).Forget();
		}
	}
}
