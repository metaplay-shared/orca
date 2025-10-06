using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Metaplay.Core.Model;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Events {
	public class DailyTaskDetails : MonoBehaviour {
		[SerializeField] private Image ActiveBackground;
		[SerializeField] private Image ClaimableBackground;
		[SerializeField] private Image ClaimedBackground;
		[SerializeField] private RectTransform Glow;
		[SerializeField] private Image Icon;
		[SerializeField] private TMP_Text ProgressLabel;
		[SerializeField] private TMP_Text Label;
		[SerializeField] private TMP_Text RewardLabel;
		[SerializeField] private Button ClaimButton;
		[SerializeField] private CanvasGroup ClaimButtonCanvasGroup;
		[SerializeField] private CanvasGroup ClaimedIconCanvasGroup;
		[SerializeField] private CanvasGroup ClaimedOverlayCanvasGroup;
		[SerializeField] private RectTransform FloatingClaimedRewardLocation;

		public DailyTaskItem Model { get; private set; }
		private EventId eventId;
		private DailyTasksUIRoot.ICallbacks callbacks;

		// ReSharper disable once ParameterHidesMember
		public void Setup(EventId eventId, DailyTaskItem model, DailyTasksUIRoot.ICallbacks callbacks) {
			this.eventId = eventId;
			Model = model;
			this.callbacks = callbacks;

			ActiveBackground.gameObject.SetActive(!model.Completed);
			ClaimableBackground.gameObject.SetActive(model.Completed && !model.RewardClaimed);
			ClaimedBackground.gameObject.SetActive(model.Completed && model.RewardClaimed);
			ClaimButtonCanvasGroup.gameObject.SetActive(!model.RewardClaimed);
			ClaimedIconCanvasGroup.gameObject.SetActive(model.RewardClaimed);
			ClaimedOverlayCanvasGroup.gameObject.SetActive(model.RewardClaimed);

			if (model.Completed && !model.RewardClaimed) {
				Glow.gameObject.SetActive(true);
				ClaimButton.interactable = true;
			} else {
				Glow.gameObject.SetActive(false);
				ClaimButton.interactable = false;
				ClaimButton.gameObject.SetActive(!model.RewardClaimed);
			}

			ProgressLabel.text = $"{model.CompletedAmount}/{model.TaskInfo.Amount}";
			Label.text = Localizer.Localize($"Event.DailyTasks.Task.{model.TaskInfo.Type}");
			RewardLabel.text = Localizer.Localize(
				"Event.DailyTasks.Reward",
				model.TaskInfo.Reward.Type,
				model.TaskInfo.Reward.Amount
			);
			Icon.SetSpriteFromAddressableAssetsAsync(
				model.TaskInfo.Icon,
				gameObject.GetCancellationTokenOnDestroy()
			).Forget();
		}

		public void OnClaimClicked() {
			MetaActionResult result = MetaplayClient.PlayerContext.ExecuteAction(new PlayerClaimDailyTaskReward(eventId, Model.Slot));
			if (result == MetaActionResult.Success) {
				ShowClaimTransition();
			}
		}

		private void ShowClaimTransition() {
			callbacks.ShowClaimedReward(
				$"+ <sprite name={Model.TaskInfo.Reward.Type}> {Model.TaskInfo.Reward.Amount}",
				FloatingClaimedRewardLocation.position
			);
			ClaimButton.interactable = false;
			ClaimedOverlayCanvasGroup.gameObject.SetActive(true);
			ClaimedBackground.gameObject.SetActive(true);
			ClaimedIconCanvasGroup.gameObject.SetActive(true);
			DOTween.Sequence()
				.Join(ClaimedBackground.DOFade(1, 0.3f).From(0))
				.Join(ClaimedOverlayCanvasGroup.DOFade(1, 0.3f).From(0))
				.Join(ClaimButtonCanvasGroup.DOFade(0, 0.3f))
				.Join(ClaimButtonCanvasGroup.transform.DOLocalMoveX(50, 0.3f).SetRelative(true))
				.Join(ClaimedIconCanvasGroup.DOFade(1, 0.3f))
				.Join(ClaimedIconCanvasGroup.transform.DOLocalMoveX(-50, 0.3f).From(isRelative: true))
				;
		}
	}
}
