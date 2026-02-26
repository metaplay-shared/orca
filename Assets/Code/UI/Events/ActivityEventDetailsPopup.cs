using System.Threading;
using Code.Purchasing;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events {
	public class ActivityEventDetailsPopup : UIRootBase<EventAdvertisementPopupPayload> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private TMP_Text Title;
		[SerializeField] private TMP_Text Description;
		[SerializeField] private EventTimer Timer;
		[SerializeField] private GameObject PremiumPassContainer;
		[SerializeField] private RewardRoad RewardRoad;
		[SerializeField] private CurrencyLabel PremiumPassCost;
		[SerializeField] private RectTransform NextGoalContainer;
		[SerializeField] private TMP_Text ScoreText;

		[Inject] private SignalBus signalBus;
		[Inject] private IPurchasingFlowController purchasingFlowController;

		private ActivityEventModel model;

		protected override void Init() {
			model = (ActivityEventModel)UIHandle.EventModel;
			RewardRoad.Setup(model);
			Title.text = Localizer.Localize("Event.ActivityEvent." + model.Info.ActivityEventType + ".Title");
			Description.text =
				Localizer.Localize("Event.ActivityEvent." + model.Info.ActivityEventType + ".Description");
			Timer.Setup(model.ActivableParams);
			PremiumPassContainer.SetActive(!model.HasPremiumPass());

			ResourceInfo premiumPassCost = model.Info.PremiumPassPrice;
			PremiumPassCost.Set(premiumPassCost.Type, premiumPassCost.Amount);
			UpdateScoreText();
		}

		protected override async UniTask Idle(CancellationToken ct) {
			await UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		private void OnEnable() {
			signalBus.Subscribe<ActivityEventPremiumPassBoughtSignal>(OnPremiumPassPurchased);
			signalBus.Subscribe<EventStateChangedSignal>(OnStateChanged);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<ActivityEventPremiumPassBoughtSignal>(OnPremiumPassPurchased);
			signalBus.Unsubscribe<EventStateChangedSignal>(OnStateChanged);
		}

		private void OnStateChanged(EventStateChangedSignal signal) {
			if (signal.EventId == model.ActivableId) {
				RewardRoad.UpdateState(model);
				UpdateScoreText();
			}
		}

		private void UpdateScoreText() {
			ActivityEventLevelInfo levelInfo = MetaplayClient.PlayerModel.GameConfig.ActivityEventLevels[
				new LevelId<EventId>(
					model.ActivableId,
					model.LastSeenLevel
				)];
			if (model.EventLevel.HasNextLevel(MetaplayClient.PlayerModel.GameConfig)) {
				ScoreText.text = $"{model.EventLevel.CurrentXp} / {levelInfo.XpToNextLevel}";
			} else {
				Description.text = Localizer.Localize("Event.Completed");
				NextGoalContainer.gameObject.SetActive(false);
			}
		}

		private void OnPremiumPassPurchased(ActivityEventPremiumPassBoughtSignal signal) {
			if (signal.EventId == model.ActivableId) {
				PremiumPassContainer.SetActive(!model.HasPremiumPass());
				RewardRoad.UpdateState(model);
			}
		}

		public void PremiumPassClicked() {
			ResourceInfo premiumPassCost = model.Info.PremiumPassPrice;

			purchasingFlowController.TrySpendGemsAsync(premiumPassCost.Amount, CancellationToken.None)
				.ContinueWith(
					success => {
						if (success) {
							MetaplayClient.PlayerContext.ExecuteAction(
								new PlayerPurchaseActivityEventPremiumPass(model.ActivableId)
							);
						}
					}
				)
				.Forget();
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}
	}
}
