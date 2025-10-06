using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.Activables;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.Events.EventCards {
	public class DailyTasksEventCard : EventCard {
		[SerializeField] private TMP_Text ScoreText;
		[Inject] private IUIRootController uiRootController;

		public override void Setup(IEventModel eventModel) {
			base.Setup(eventModel);

			if (EventModel is DailyTaskEventModel dailyTaskEventModel) {
				ScoreText.text = $"{dailyTaskEventModel.CompletedCount}/{dailyTaskEventModel.Tasks.Count}";
			}
		}

		protected override void OnClick() {
			if (EventModel is DailyTaskEventModel dailyTaskEventModel) {
				bool isActive =
					EventModel.Status(MetaplayClient.PlayerModel) is not MetaActivableVisibleStatus.InPreview;
				if (isActive) {
					uiRootController.ShowUI<DailyTasksUIRoot, DailyTasksUIHandle>(
						new DailyTasksUIHandle(dailyTaskEventModel),
						gameObject.GetCancellationTokenOnDestroy()
					).OnComplete.Forget();
				} else {
					base.OnClick();
				}
			}
		}
	}
}
