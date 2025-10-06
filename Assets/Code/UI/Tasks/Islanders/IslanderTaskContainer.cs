using System.Collections.Generic;
using Code.UI.Application;
using Code.UI.Tasks.Signals;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UniRx;
using UnityEngine;
using Zenject;

namespace Code.UI.Tasks.Islanders {
	public class IslanderTaskContainer : MonoBehaviour {
		[SerializeField] private IslanderTaskGiver TemplateTaskGiver;
		[SerializeField] private RectTransform TaskGiverContainer;

		[Inject] private DiContainer container;
		[Inject] private SignalBus signalBus;
		[Inject] private ApplicationInfo applicationInfo;

		private readonly List<GameObject> taskGivers = new();

		private void OnEnable() {
			signalBus.Subscribe<IslandTaskModifiedSignal>(UpdateTaskGivers);
			applicationInfo.ActiveIsland
				.TakeUntilDisable(this)
				.Subscribe(_ => UpdateTaskGivers());
		}

		private void OnDisable() {
			signalBus.Unsubscribe<IslandTaskModifiedSignal>(UpdateTaskGivers);
		}

		private void UpdateTaskGivers() {
			foreach (GameObject taskGiver in taskGivers) {
				Destroy(taskGiver);
			}
			taskGivers.Clear();

			if (!MetaplayClient.PlayerModel.Islands.TryGetValue(applicationInfo.ActiveIsland.Value, out IslandModel islandModel)) {
				return;
			}

			if (islandModel.Tasks?.Tasks == null) {
				return;
			}

			foreach (var taskGiverKvp in islandModel.Tasks.Tasks) {
				IslanderId islanderId = taskGiverKvp.Key;
				IslandTaskModel islandTaskModel = taskGiverKvp.Value;

				if (!islandTaskModel.Enabled) {
					continue;
				}

				IslanderTaskGiver instance =
					container.InstantiatePrefabForComponent<IslanderTaskGiver>(TemplateTaskGiver, TaskGiverContainer);
				taskGivers.Add(instance.gameObject);

				instance.Setup(islanderId, islandTaskModel);
			}
		}
	}
}
