using Code.Logbook;
using Code.UI.Core;
using Code.UI.Logbook.Tasks;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using System.Threading;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Logbook {
	public class LogbookUIRoot : UIRootWithResultBase<LogbookUIHandle, LogbookUIResult> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private TMP_Text Title;
		[SerializeField] private GameObject DiscoveryNotification;
		[SerializeField] private GameObject TasksNotification;
		[SerializeField] private ToggleGroup ToggleGroup;
		[SerializeField] private Toggle DiscoveryToggle;
		[SerializeField] private Toggle TasksToggle;

		[Inject] private IItemDiscoveryController itemDiscoveryController;
		[Inject] private ILogbookTasksController logbookTasksController;

		protected void Awake() {
			itemDiscoveryController.HasPendingRewards
				.Subscribe(active => DiscoveryNotification.SetActive(active))
				.AddTo(gameObject);
			logbookTasksController.HasPendingRewards
				.Subscribe(active => TasksNotification.SetActive(active))
				.AddTo(gameObject);
			DiscoveryToggle.OnValueChangedAsObservable()
				.Subscribe(
					isOn => {
						if (isOn) {
							Title.text = Localizer.Localize("Discovery.Title");
						}
					}
				)
				.AddTo(gameObject);
			TasksToggle.OnValueChangedAsObservable()
				.Subscribe(
					isOn => {
						if (isOn) {
							Title.text = Localizer.Localize("Logbook.Title");
						}
					}
				)
				.AddTo(gameObject);
		}

		protected override void Init() {
			ToggleGroup.SetAllTogglesOff();
			if (logbookTasksController.HasPendingRewards.Value) {
				TasksToggle.isOn = true;
			} else if (itemDiscoveryController.HasPendingRewards.Value) {
				DiscoveryToggle.isOn = true;
			} else {
				TasksToggle.isOn = true;
			}
		}

		protected override async UniTask<LogbookUIResult> IdleWithResult(CancellationToken ct) {
			(int _, LogbookUIResult result) resTuple =  await UniTask.WhenAny(
				new [] {
					OnDismissAsync(ct).ContinueWith(() => new LogbookUIResult()),
					UIHandle.OnNavigationRequested.ContinueWith(res => (LogbookUIResult)res)
				}
			);
			return resTuple.result;
		}

		private UniTask OnDismissAsync(CancellationToken ct) {
			return UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}
	}

	public class LogbookUIHandle : UIHandleWithResultBase<LogbookUIResult>, ILogbookTaskNavigationMediator {
		private readonly UniTaskCompletionSource<NavigateToTaskResult> navigationRequestedTCS = new ();

		public UniTask<NavigateToTaskResult> OnNavigationRequested => navigationRequestedTCS.Task;

		public void NavigateToTask(LogbookTaskModel taskModel) {
			navigationRequestedTCS.TrySetResult(new NavigateToTaskResult(taskModel));
		}

		protected override void OnCancel() {
			base.OnCancel();
			navigationRequestedTCS.TrySetCanceled();
		}
	}

	public class LogbookUIResult : IUIResult { }

	public class NavigateToTaskResult : LogbookUIResult {
		public LogbookTaskModel TaskModel { get; }

		public NavigateToTaskResult(
			LogbookTaskModel taskModel
		) {
			TaskModel = taskModel;
		}
	}
}
