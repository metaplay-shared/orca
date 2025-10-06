using System;
using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events {
	public class ActivityEventButton : EventButton {
		[SerializeField] private Image ProgressFill;
		[SerializeField] private TMP_Text ProgressText;

		[Inject] private SignalBus signalBus;

		private PlayerModel PlayerModel => MetaplayClient.PlayerModel;
		private SharedGameConfig GameConfig => MetaplayClient.PlayerModel.GameConfig;

		public override void SetupEventModel(IEventModel eventModel, Action<EventButton> eventButtonClickCallback) {
			base.SetupEventModel(eventModel, eventButtonClickCallback);
			signalBus.Subscribe<ActivityEventScoreAddedSignal>(UpdateActivityEventScore);
			if (eventModel is ActivityEventModel activityEventModel) {
				UpdateProgress(activityEventModel);
			}
		}

		private void OnDestroy() {
			signalBus?.TryUnsubscribe<ActivityEventScoreAddedSignal>(UpdateActivityEventScore);
		}

		private void UpdateActivityEventScore(ActivityEventScoreAddedSignal signal) {
			IEventModel eventModel = PlayerModel.TryGetEventModel(signal.EventId);
			if (eventModel is ActivityEventModel activityEventModel) {
				UpdateProgress(activityEventModel);
			}
		}

		private void UpdateProgress(ActivityEventModel activityEventModel) {
			if (activityEventModel.EventLevel.HasNextLevel(MetaplayClient.PlayerModel.GameConfig)) {
				int activityEventScore = activityEventModel.EventLevel.CurrentXp;
				int activityEventTargetScore = activityEventModel.EventLevel.GetLevelInfo(GameConfig).XpToNextLevel;
				float normalizedProgress = (float)activityEventScore / activityEventTargetScore;

				ProgressFill.fillAmount = normalizedProgress;
				ProgressText.text = $"{activityEventScore}/{activityEventTargetScore}";
			} else {
				ProgressFill.fillAmount = 1;
				ProgressText.text = Localizer.Localize("Event.ActivityEvent.Complete");
			}
		}
	}
}
