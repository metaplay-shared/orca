using System;
using System.Linq;
using System.Threading;
using Code.UI.AssetManagement;
using Code.UI.MergeBase.Signals;
using Code.UI.RequirementsDisplay;
using Code.UI.Tasks.Signals;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Tasks.Islanders {
	public class IslanderTaskGiver : ButtonHelper {
		[SerializeField] private GameObject GroupCompleted;
		[SerializeField] private Image CharacterImage;
		[SerializeField] private ResourceRequirements ResourceRequirements;
		[SerializeField] private RectTransform CompletionMarkerContainer;

		[SerializeField] private TaskGiverProgressionItem TaskGiverProgressionItem;

		[Inject] private DiContainer container;
		[Inject] private AddressableManager addressableManager;

		private bool IsLastTask {
			get {
				var tasks = MetaplayClient.PlayerModel.GameConfig.GetIslanderTasks(
					model.Info.Islander,
					model.Info.GroupId
				);

				if (CurrentTask.Info.Id == tasks.Last().Id) {
					return true;
				}

				return false;
			}
		}

		private bool HasAllRequirementsFulfilled {
			get {
				var board = MetaplayClient.PlayerModel.Islands[model.Info.Island].MergeBoard;
				return board.HasItemsForTask(model.Info);
			}
		}

		private IslandTaskModel model;
		private IslanderId islander;

		private IslandTaskModel CurrentTask =>
			MetaplayClient.PlayerModel.Islands[model.Info.Island].Tasks.Tasks[islander];

		public void Setup(
			IslanderId islanderId,
			IslandTaskModel taskGiverModel
		) {
			islander = islanderId;
			model = taskGiverModel;

			container.Inject(ResourceRequirements);

			GroupCompleted.gameObject.SetActive(HasAllRequirementsFulfilled);
			ResourceRequirements.gameObject.SetActive(true);
			SetRequirementsAsync(default).Forget();
			SetCompletionState();

			SetupCharacterImage();
		}

		private void SetCompletionState() {
			var tasks = MetaplayClient.PlayerModel.GameConfig.GetIslanderTasks(model.Info.Islander, model.Info.GroupId);

			foreach (Transform child in CompletionMarkerContainer) {
				Destroy(child.gameObject);
			}

			int taskUiIndex = 1;
			foreach (IslandTaskInfo task in tasks) {
				TaskGiverProgressionItem taskGiverProgressionItem =
					container.InstantiatePrefabForComponent<TaskGiverProgressionItem>(
						TaskGiverProgressionItem,
						CompletionMarkerContainer
					);
				taskGiverProgressionItem.Setup(task, CurrentTask, taskUiIndex);
				taskUiIndex++;
			}
		}

		private void SetupCharacterImage() {
			try {
				CharacterImage.sprite = addressableManager.Get<Sprite>($"Heroes/{model.Info.Islander}.png");
			} catch (Exception e) {
				Debug.LogException(e);
				CharacterImage.sprite = null;
			}
		}

		private async UniTask SetRequirementsAsync(CancellationToken ct) {
			if (!MetaplayClient.PlayerModel.Islands[model.Info.Island].Tasks.Tasks.ContainsKey(islander)) {
				return;
			}

			IslandTaskModel task = MetaplayClient.PlayerModel.Islands[model.Info.Island].Tasks.Tasks[islander];

			var requirementItems = task.Info.Items.Select(
				r => new RequirementItemItem() {
					RequiredAmount = r.Count,
					TypeName = r.Type.Value,
					ItemLevel = r.Level
				}
			).ToArray();

			await ResourceRequirements.SetupAsync(requirementItems, model.Info.Island);
		}

		protected override void OnEnable() {
			base.OnEnable();
			signalBus.Subscribe<IslandTaskModifiedSignal>(OnIslandTaskModified);
			signalBus.Subscribe<ItemsOnBoardChangedSignal>(OnItemsChanged);
		}

		protected override void OnDisable() {
			base.OnDisable();
			signalBus.Unsubscribe<IslandTaskModifiedSignal>(OnIslandTaskModified);
			signalBus.Unsubscribe<ItemsOnBoardChangedSignal>(OnItemsChanged);
		}

		private void OnItemsChanged(ItemsOnBoardChangedSignal signal) {
			if (!MetaplayClient.PlayerModel.Islands[model.Info.Island].Tasks.Tasks.ContainsKey(islander)) {
				return;
			}

			ResourceRequirements.gameObject.SetActive(true);
			GroupCompleted.gameObject.SetActive(HasAllRequirementsFulfilled);
			SetRequirementsAsync(default).Forget();
		}

		private void OnIslandTaskModified(IslandTaskModifiedSignal signal) {
			SetRequirementsAsync(default).Forget();
			SetCompletionState();
		}

		private bool CanComplete(IslandTaskInfo taskInfo) {
			return MetaplayClient.PlayerModel.Islands[model.Info.Island].MergeBoard.HasItemsForTask(taskInfo);
		}

		protected override void OnClick() {
			IslandTaskModel task = MetaplayClient.PlayerModel.Islands[model.Info.Island].Tasks.Tasks[islander];
			if (CanComplete(task.Info)) {
				MetaplayClient.PlayerContext.ExecuteAction(
					new PlayerFulfillIslandTask(model.Info.Island, model.Info.Islander)
				);
			}
		}
	}
}
