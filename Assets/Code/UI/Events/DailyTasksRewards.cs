using System.Collections.Generic;
using Game.Logic;
using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Code.UI.Events {
	public class DailyTasksRewards : MonoBehaviour {
		[SerializeField] private DailyTasksReward RewardTemplate;
		[SerializeField] private GameObject SpacerTemplate;

		[Inject] private DiContainer container;
		[Inject] private SignalBus signalBus;

		private List<DailyTasksReward> Rewards = new List<DailyTasksReward>();

		public void Setup(DailyTaskEventModel model) {
			for (var i = 0; i < model.Info.Rewards.Count; i++) {
				LevelId<ChainTypeId> item = model.Info.Rewards[i];
				DailyTasksReward reward =
					container.InstantiatePrefabForComponent<DailyTasksReward>(RewardTemplate, transform);

				int i1 = i;
				bool IsClaimed() => i1 < model.Level || (i1 == model.Level && model.Completed() && model.UnclaimedRewards() == 0);
				bool IsCurrent() => i1 == model.Level;

				reward.UpdateVisuals(item, IsClaimed(), IsCurrent());

				if (i < model.Info.Rewards.Count - 1) {
					Instantiate(SpacerTemplate, transform);
				}

				Rewards.Add(reward);

				Observable.FromEvent(
						handler => signalBus.Subscribe<EventStateChangedSignal>(handler),
						handler => signalBus.Unsubscribe<EventStateChangedSignal>(handler)
					)
					.Subscribe(_ => reward.UpdateVisuals(item, IsClaimed(), IsCurrent()))
					.AddTo(gameObject);
			}
		}
	}
}
