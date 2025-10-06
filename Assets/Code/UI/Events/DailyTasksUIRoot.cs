using System.Threading;
using Code.UI.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Events {
	public class DailyTasksUIHandle : UIHandleBase {
		public readonly DailyTaskEventModel Model;

		public DailyTasksUIHandle(DailyTaskEventModel model) {
			Model = model;
		}
	}

	public class DailyTasksUIRoot : UIRootBase<DailyTasksUIHandle>, DailyTasksUIRoot.ICallbacks {
		public interface ICallbacks {
			public void ShowClaimedReward(string text, Vector3 position);
		}

		[SerializeField] private Button CloseButton;
		[SerializeField] private TMP_Text Title;
		[SerializeField] private EventTimer Timer;
		[SerializeField] private DailyTasksRewards Rewards;
		[SerializeField] private DailyTasksTaskList Tasks;
		[SerializeField] private RectTransform FloatingClaimedRewardContainer;
		[SerializeField] private FloatingClaimedReward FloatingClaimedRewardTemplate;
		[SerializeField] private Slider TaskProgressSlider;
		[SerializeField] private TMP_Text TaskProgressSliderLabel;

		private DailyTaskEventModel model;

		protected override void Init() {
			Rewards.Setup(UIHandle.Model);
			Tasks.Setup(UIHandle.Model, callbacks: this);
			// TODO: Use localization
			Title.text = "Daily Tasks";// Localizer.Localize("Event.Title.DailyTasks");
			Timer.Setup(UIHandle.Model.ActivableParams);
			FloatingClaimedRewardTemplate.gameObject.SetActive(false);

			int totalTasksCount = UIHandle.Model.Tasks.Count;
			// Do not count unclaimed tasks as completed here for more intuitive UX
			int completedTasksCount = UIHandle.Model.CompletedCount - UIHandle.Model.UnclaimedRewards();
			TaskProgressSlider.maxValue = totalTasksCount;
			TaskProgressSlider.value = completedTasksCount;
			// TODO: Use localization
			TaskProgressSliderLabel.text = $"Completed tasks: {completedTasksCount} / {totalTasksCount}";
		}

		protected override UniTask Idle(CancellationToken ct) {
			return UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}

		void ICallbacks.ShowClaimedReward(string text, Vector3 position) {
			int totalTasksCount = UIHandle.Model.Tasks.Count;
			// Do not count unclaimed tasks as completed here for more intuitive UX
			int completedTasksCount = UIHandle.Model.CompletedCount - UIHandle.Model.UnclaimedRewards();
			TaskProgressSlider.DOValue(completedTasksCount, 1.0f);
			// TODO: Use localization
			TaskProgressSliderLabel.text = $"Completed tasks: {completedTasksCount} / {totalTasksCount}";
			FloatingClaimedReward floatingReward = Instantiate(
				FloatingClaimedRewardTemplate,
				FloatingClaimedRewardContainer
			);
			floatingReward.gameObject.SetActive(true);
			floatingReward.transform.position = position;
			floatingReward.Show(text);
		}
	}
}
