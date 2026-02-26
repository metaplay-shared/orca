using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using Orca.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events {
	public class RewardRoad : MonoBehaviour {
		[SerializeField] private Slider LevelSlider;
		[SerializeField] private RectTransform LevelsContainer;
		[SerializeField] private RewardRoadLevel LevelTemplate;
		[SerializeField] private ScrollRect ScrollRect;

		[Inject] private SignalBus signalBus;
		[Inject] private DiContainer diContainer;

		private List<RewardRoadLevel> roadLevels = new();
		private Option<EventId> eventId;

		private void OnEnable() {
			signalBus.Subscribe<ActivityEventRewardClaimedSignal>(OnRewardClaimed);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<ActivityEventRewardClaimedSignal>(OnRewardClaimed);
		}

		public void Setup(ActivityEventModel model) {
			eventId = model.ActivableId;
			roadLevels = new List<RewardRoadLevel>();
			List<ActivityEventLevelInfo> levels =
				MetaplayClient.PlayerModel.GameConfig.ActivityEventLevelsByEvent
					.GetValueOrDefault(model.ActivableId)
					.Where(l => l.Level > 0)
					.OrderBy(l => l.Level)
					.ToList();

			foreach (ActivityEventLevelInfo level in levels
			) {
				RewardRoadLevel reward =
					diContainer.InstantiatePrefabForComponent<RewardRoadLevel>(LevelTemplate, LevelsContainer);
				reward.Setup(model, level);
				roadLevels.Add(reward);
			}
			// The template is placed in the hierarchy for easier preview
			Destroy(LevelTemplate.gameObject);

			LevelSlider.maxValue = levels.Count;

			SetProgress(model);
		}

		public void UpdateState(ActivityEventModel model) {
			foreach (RewardRoadLevel level in roadLevels) {
				ActivityEventLevelInfo levelInfo =
					MetaplayClient.PlayerModel.GameConfig.ActivityEventLevels[new LevelId<EventId>(
						model.ActivableId,
						level.Level
					)];
				level.Setup(model, levelInfo);
			}
		}

		private void SetProgress(ActivityEventModel model) {
			float lastSeenValue = GetLastSeenValueFromModel(model);
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerUpdateActivityEventLastSeen(model.ActivableId));
			float currentValue = GetLastSeenValueFromModel(model);

			static float GetLastSeenValueFromModel(ActivityEventModel model) {
				ActivityEventLevelInfo lastSeenLevelInfo = MetaplayClient.PlayerModel.GameConfig.ActivityEventLevels[
					new LevelId<EventId>(
						model.ActivableId,
						model.LastSeenLevel
					)];
				float lastSeenLevelProgress = lastSeenLevelInfo.XpToNextLevel > 0
					? (float) model.LastSeenScore / lastSeenLevelInfo.XpToNextLevel
					: 0f;
				float lastSeenValue = lastSeenLevelInfo.Level + lastSeenLevelProgress;
				return lastSeenValue;
			}

			float lastSeenNormalizedValue = lastSeenValue / LevelSlider.maxValue;
			float currentNormalizedValue = currentValue / LevelSlider.maxValue;

			ScrollRect.horizontalNormalizedPosition = lastSeenNormalizedValue;
			LevelSlider.value = lastSeenValue;
			DOTween.Sequence()
				.AppendInterval(0.5f)
				.Append(LevelSlider.DOValue(currentValue, 1.0f))
				.Join(ScrollRect.DOHorizontalNormalizedPos(currentNormalizedValue, 1.0f));

		}

		private void OnRewardClaimed(ActivityEventRewardClaimedSignal signal) {
			if (!eventId.Exists(signal.EventId, (signalId, id) => id == signalId)) {
				return;
			}

			RewardRoadLevel level = roadLevels.Find(l => l.Level == signal.Level);
			if (level == null) {
				return;
			}

			level.FlyRewards(signal.Premium);
		}
	}
}
