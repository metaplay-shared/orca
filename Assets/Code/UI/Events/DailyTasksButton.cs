using Code.DailyTasks;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Code.UI.Events {
	public class DailyTasksButton : ButtonHelper {
		[SerializeField] private EventTimer EventTimer;
		[SerializeField] private GameObject NewIndicator;

		[Inject] private IUIRootController uiRootController;
		[Inject] private IDailyTasksController dailyTasksController;

		protected void Start() {
			dailyTasksController.HasSomethingToClaim.Subscribe(HandleHasSomethingToClaim).AddTo(gameObject);
		}

		protected override void OnEnable() {
			base.OnEnable();

			SetupTimer();
			signalBus.Subscribe<EventStateChangedSignal>(OnEventStateChanged);
		}

		protected override void OnDisable() {
			base.OnDisable();
			signalBus.Unsubscribe<EventStateChangedSignal>(OnEventStateChanged);
		}

		private void OnEventStateChanged(EventStateChangedSignal signal) {
			SetupTimer();
		}

		private void SetupTimer() {
			EventTimer.Setup(dailyTasksController.GetDailyTaskEventModel()?.EventInfo.ActivableParams);
		}

		protected override void OnClick() {
			uiRootController.ShowUI<DailyTasksUIRoot, DailyTasksUIHandle>(
				new DailyTasksUIHandle(dailyTasksController.GetDailyTaskEventModel()),
				gameObject.GetCancellationTokenOnDestroy()
			);
		}

		private void HandleHasSomethingToClaim(bool active) {
			NewIndicator.SetActive(active);
		}
	}
}
