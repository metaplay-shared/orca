using Code.Logbook;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Logbook.Tasks {
	public class LogbookTaskProgressionPresenter : MonoBehaviour {
		[SerializeField] private Button ClaimButton;
		[SerializeField] private TMP_Text Subtitle;
		[SerializeField] private ChapterProgressionWaypointPresenter TemplateWaypointPresenter;
		[SerializeField] private Transform WaypointsParent;
		[SerializeField] private Slider RewardRoadSlider;
		[SerializeField] private Image RewardIcon;
		[SerializeField] private ScrollRect ScrollRect;

		[Inject] private DiContainer container;
		[Inject] private ILogbookTasksController logbookTasksController;

		private readonly List<ChapterProgressionWaypointPresenter> waypoints = new();
		private LogbookChapterModel chapterModel;

		private void OnEnable() {
			ClaimButton.onClick.AddListener(HandleClaimButtonClicked);
			logbookTasksController.ChapterModified += HandleChapterModified;
			logbookTasksController.TaskModified += HandleTaskModified;
		}

		private void OnDisable() {
			ClaimButton.onClick.RemoveListener(HandleClaimButtonClicked);
			logbookTasksController.ChapterModified -= HandleChapterModified;
			logbookTasksController.TaskModified -= HandleTaskModified;
		}

		private void HandleClaimButtonClicked() {
			logbookTasksController.ClaimChapterReward(chapterModel.Info.Id);
		}

		private void HandleTaskModified(LogbookTaskId taskId) {
			if (chapterModel.Tasks.ContainsKey(taskId)) {
				UpdateVisualisation();
			}
		}

		private void HandleChapterModified(LogbookChapterId chapterId) {
			if (chapterId == chapterModel.Info.Id) {
				UpdateVisualisation();
			}
		}

		public void Setup(LogbookChapterModel chapter) {
			chapterModel = chapter;
			UpdateVisualisation();
		}

		private void UpdateVisualisation() {
			int completedTasksCount = chapterModel.Tasks.Values.Count(task => task.IsComplete);
			int totalTasksCount = chapterModel.Tasks.Count;

			ClaimButton.gameObject.SetActive(chapterModel.State == ChapterState.Complete);
			Subtitle.text = Localizer.Localize(
				"Logbook.ChapterProgression.Subtitle",
				completedTasksCount.ToString(CultureInfo.InvariantCulture),
				totalTasksCount.ToString(CultureInfo.InvariantCulture)
			);
			SetupRewardRoad(chapterModel);
		}

		private void SetupRewardRoad(LogbookChapterModel chapter) {
			RewardRoadSlider.minValue = 1;
			RewardRoadSlider.maxValue = chapter.Tasks.Count;
			RewardRoadSlider.value = chapter.Tasks.Values.Count(task => task.IsComplete);
			SetupWaypoints(chapter);
			SetupRewardIcon(gameObject.GetCancellationTokenOnDestroy()).Forget();

			async UniTask SetupRewardIcon(CancellationToken ct) {
				RewardIcon.enabled = false;

				AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(chapter.Info.RewardIcon);
				ct.Register(() => Addressables.Release(handle));
				Sprite sprite = await handle;
				ct.ThrowIfCancellationRequested();
				RewardIcon.sprite = sprite;

				RewardIcon.enabled = true;
			}
		}

		private void SetupWaypoints(LogbookChapterModel chapter) {
			foreach (ChapterProgressionWaypointPresenter waypoint in waypoints) {
				Destroy(waypoint.gameObject);
			}
			waypoints.Clear();

			int completedTasksCount = chapter.Tasks.Count(task => task.Value.IsComplete);
			for (var i = 0; i < chapter.Tasks.Count; i++) {
				ChapterProgressionWaypointPresenter waypoint =
					container.InstantiatePrefabForComponent<ChapterProgressionWaypointPresenter>(
						TemplateWaypointPresenter,
						WaypointsParent
					);
				waypoint.SetActive(i < completedTasksCount);
				waypoints.Add(waypoint);
			}

			SetNormalizedPositionDelayed(gameObject.GetCancellationTokenOnDestroy()).Forget();

			async UniTask SetNormalizedPositionDelayed(CancellationToken getCancellationTokenOnDestroy) {
				await UniTask.DelayFrame(3, cancellationToken: getCancellationTokenOnDestroy);
				float normalizedPosition = completedTasksCount / (float)chapter.Tasks.Count;
				ScrollRect.horizontalNormalizedPosition = normalizedPosition;
			}
		}
	}
}
