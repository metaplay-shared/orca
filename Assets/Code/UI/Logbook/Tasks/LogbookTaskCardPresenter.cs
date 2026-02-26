using System;
using Code.Logbook;
using Code.UI.AssetManagement;
using Code.UI.Effects;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using System.Globalization;
using System.Threading;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Logbook.Tasks {
	public interface ILogbookTaskNavigationMediator {
		void NavigateToTask(LogbookTaskModel taskModel);
	}

	public class LogbookTaskCardPresenter : MonoBehaviour {
		[SerializeField] private GameObject InactiveBackground;
		[SerializeField] private GameObject ActiveBackground;
		[SerializeField] private GameObject CompleteBackground;
		[SerializeField] private GameObject ActiveContent;
		[SerializeField] private GameObject InactiveContent;
		[SerializeField] private Image RequirementIcon;
		[SerializeField] private TMP_Text RequirementText;
		[SerializeField] private TMP_Text Title;
		[SerializeField] private TMP_Text SubtitleText;
		[SerializeField] private GameObject ButtonsContainer;
		[SerializeField] private Button FindButton;
		[SerializeField] private Button ClaimButton;

		[Inject] private LogbookTaskModel taskModel;
		[Inject] private ILogbookTaskNavigationMediator navigationMediator;
		[Inject] private ILogbookTasksController logbookTasksController;
		[Inject] private EffectsController effectsController;

		private void Awake() {
			Setup();
		}

		private void OnEnable() {
			FindButton.onClick.AddListener(HandleFindButtonClicked);
			ClaimButton.onClick.AddListener(HandleClaimButtonClicked);
			logbookTasksController.TaskModified += HandleTaskModified;
		}

		private void OnDisable() {
			FindButton.onClick.RemoveListener(HandleFindButtonClicked);
			ClaimButton.onClick.RemoveListener(HandleClaimButtonClicked);
			logbookTasksController.TaskModified -= HandleTaskModified;
		}

		private void HandleFindButtonClicked() {
			navigationMediator.NavigateToTask(taskModel);
		}

		private void HandleClaimButtonClicked() {
			logbookTasksController.ClaimTaskReward(taskModel.Info.Id);
			effectsController.FlyCurrencyParticles(
				taskModel.Info.Reward.Type,
				taskModel.Info.Reward.Amount,
				ClaimButton.transform.position
			).Forget();
		}

		private void HandleTaskModified(LogbookTaskId taskId) {
			if (taskModel.Info.Id == taskId) {
				Setup();
			}
		}

		private void Setup() {
			SetupBackground();
			SetupRequirements();
			SetupContent();
			SetupButtons();
		}

		private void SetupButtons() {
			ButtonsContainer.SetActive(taskModel.IsOpen && !taskModel.IsClaimed);
			FindButton.gameObject.SetActive(!taskModel.IsComplete && taskModel.Info.Operations.Count > 0);
			ClaimButton.gameObject.SetActive(taskModel.IsComplete && !taskModel.IsClaimed);
		}

		private void SetupRequirements() {
			RequirementText.text = $"{taskModel.Count}/{taskModel.Info.Count}";
			SetupRequirementIcon(gameObject.GetCancellationTokenOnDestroy()).Forget();

			async UniTask SetupRequirementIcon(CancellationToken ct) {
				RequirementIcon.enabled = false;

				AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(ResolveIconAddress());
				ct.Register(() => Addressables.Release(handle));
				Sprite sprite = await handle;
				ct.ThrowIfCancellationRequested();
				RequirementIcon.sprite = sprite;

				RequirementIcon.enabled = true;
			}
		}

		private string ResolveIconAddress() {
			if (taskModel.Info.Icon?.Length > 0) {
				return taskModel.Info.Icon;
			}

			if (taskModel.Info.Item.Type != ChainTypeId.None) {
				int level = Math.Max(taskModel.Info.Item.Level, 1);
				return AddressableUtils.GetItemIconAddress(taskModel.Info.Item.Type, level);
			}

			return "Icons/Icon_Inventory.png";
		}

		private void SetupContent() {
			InactiveContent.SetActive(!taskModel.IsOpen);
			ActiveContent.SetActive(taskModel.IsOpen);

			Title.text = Localizer.Localize(taskModel.Info.TitleKey);
			SubtitleText.text = !taskModel.IsClaimed
				? Localizer.Localize(
					"Logbook.TaskReward",
					taskModel.Info.Reward.Amount.ToString(CultureInfo.InvariantCulture),
					taskModel.Info.Reward.Type.Value
				)
				: Localizer.Localize(
					"Logbook.CompletedAt",
					taskModel.CompletedAt?.ToDateTime()
						.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern)
				);
		}

		private void SetupBackground() {
			InactiveBackground.SetActive(!taskModel.IsOpen);
			ActiveBackground.SetActive(taskModel.IsOpen && !taskModel.IsComplete);
			CompleteBackground.SetActive(taskModel.IsComplete);
		}
	}
}
