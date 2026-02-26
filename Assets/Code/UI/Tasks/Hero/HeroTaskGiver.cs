using System;
using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Tasks.Hero {
	public class HeroTaskGiver : ButtonHelper {
		[SerializeField] private Image TimerIcon;
		[SerializeField] private TMP_Text HeroTaskTimerText;
		[SerializeField] protected Image ProfilePicture;

		private HeroModel model;
		private Action<RectTransform, HeroModel> clickCallback;

		public void Setup(HeroInfo taskGiverModel, Action<RectTransform, HeroModel> taskGiverClickCallback) {
			clickCallback = taskGiverClickCallback;
			model = MetaplayClient.PlayerModel.Heroes.Heroes.ContainsKey(taskGiverModel.Type)
				? MetaplayClient.PlayerModel.Heroes.Heroes[taskGiverModel.Type]
				: null;
		}

		protected override void OnEnable() {
			base.OnEnable();

			signalBus.Subscribe<HeroTaskModifiedSignal>(OnHeroTaskModified);
		}

		protected override void OnDisable() {
			base.OnDisable();

			signalBus.Unsubscribe<HeroTaskModifiedSignal>(OnHeroTaskModified);
		}

		private void Update() {
			if (model == null) {
				HeroIsLocked();
				return;
			}

			ProfilePicture.color = CanComplete() ? Color.green : Color.white;

			if (model.CurrentTask is not { State: HeroTaskState.Fulfilled }) {
				HeroTaskTimerText.gameObject.SetActive(false);
				TimerIcon.gameObject.SetActive(false);

				return;
			}

			TimerIcon.gameObject.SetActive(true);
			HeroTaskTimerText.gameObject.SetActive(true);
			HeroTaskTimerText.text = (model.CurrentTask.FinishedAt - MetaplayClient.PlayerModel.CurrentTime)
				.ToSimplifiedString();
		}

		private void HeroIsLocked() {
			TimerIcon.gameObject.SetActive(false);
			HeroTaskTimerText.gameObject.SetActive(true);
			HeroTaskTimerText.text = "Locked";
		}

		private bool CanComplete() {
			if (model.CurrentTask == null) {
				return false;
			}

			return MetaplayClient.PlayerModel.Inventory.HasEnoughResources(model.CurrentTask.Info) &&
				model.CurrentTask.State == HeroTaskState.Created;
		}

		private void OnHeroTaskModified(HeroTaskModifiedSignal signal) {
			if (model == null) {
				return;
			}

			if (signal.HeroType != model.Info.Type) {
				return;
			}

			TimerIcon.gameObject.SetActive(model.CurrentTask.State == HeroTaskState.Fulfilled);
			HeroTaskTimerText.gameObject.SetActive(model.CurrentTask.State == HeroTaskState.Fulfilled);

			ProfilePicture.color = model.CurrentTask.State switch {
				HeroTaskState.Created   => Color.white,
				HeroTaskState.Fulfilled => Color.green,
				_                       => Color.black
			};
		}

		protected override void OnClick() {
			clickCallback.Invoke(GetComponent<RectTransform>(), model);
		}
	}
}
