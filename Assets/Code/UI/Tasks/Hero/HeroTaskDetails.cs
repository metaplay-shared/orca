using System.Linq;
using System.Threading;
using Code.UI.RequirementsDisplay;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.Tasks.Hero {
	public class HeroTaskDetails : TaskDetails {
		[SerializeField] private SkipTimerButton SkipButton;
		[SerializeField] private TMP_Text TaskTimerText;

		[Inject] private DiContainer container;

		private HeroModel taskGiver;

		public void Setup(HeroModel taskGiverModel) {
			container.Inject(SkipButton);
			taskGiver = taskGiverModel;

			UpdateRequirementsAsync(default).Forget();
			CompleteTaskButton.Setup(taskGiverModel.Info.Type);
			ClaimButton.Setup(taskGiverModel.Info.Type);
			SkipButton.Setup(taskGiverModel.Info.Type);

			ClaimButton.gameObject.SetActive(CanClaim());
			CompleteTaskButton.gameObject.SetActive(CanComplete());
			SkipButton.gameObject.SetActive(CanSkip());

			TaskTimerText.gameObject.SetActive(CanSkip());
		}

		private bool CanComplete() {
			return MetaplayClient.PlayerModel.Inventory.HasEnoughResources(taskGiver.CurrentTask.Info) &&
				taskGiver.CurrentTask.State == HeroTaskState.Created;
		}

		private bool CanClaim() {
			return taskGiver.CurrentTask.State == HeroTaskState.Finished;
		}

		private bool CanSkip() {
			return taskGiver.CurrentTask.State == HeroTaskState.Fulfilled;
		}

		private void OnEnable() {
			signalBus.Subscribe<ResourcesChangedSignal>(OnResourcesChanged);
			signalBus.Subscribe<HeroTaskModifiedSignal>(OnHeroTaskModified);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<ResourcesChangedSignal>(OnResourcesChanged);
			signalBus.Unsubscribe<HeroTaskModifiedSignal>(OnHeroTaskModified);
		}

		private void OnResourcesChanged(ResourcesChangedSignal signal) {
			UpdateRequirementsAsync(default).Forget();
		}

		private void OnHeroTaskModified(HeroTaskModifiedSignal signal) {
			ClaimButton.gameObject.SetActive(CanClaim());
			CompleteTaskButton.gameObject.SetActive(CanComplete());
			SkipButton.gameObject.SetActive(CanSkip());

			ResourceRequirements.gameObject.SetActive(!CanClaim() && !CanSkip());
			TaskTimerText.gameObject.SetActive(CanSkip());
			SetSkipCost();

			if (taskGiver.CurrentTask.State == HeroTaskState.Claimed) {
				gameObject.SetActive(false);
			}
		}

		private async UniTask UpdateRequirementsAsync(CancellationToken ct) {
			ClaimButton.gameObject.SetActive(CanClaim());
			CompleteTaskButton.gameObject.SetActive(CanComplete());
			SkipButton.gameObject.SetActive(CanSkip());

			var requirementItems = taskGiver.CurrentTask.Info.Resources.Select(
				r => new RequirementResourceItem() {
					RequiredAmount = r.Amount,
					TypeName = r.Type.Value
				}
			).ToArray();

			await ResourceRequirements.SetupAsync(requirementItems);
		}

		private void SetSkipCost() {
			TMP_Text skipText = SkipButton.GetComponentInChildren<TMP_Text>();

			var cost = MetaplayClient.PlayerModel.SkipHeroTaskTimerCost(taskGiver.Info.Type);
			skipText.text = $"Skip timer\n<sprite=3> {cost}";
			skipText.text = Localizer.Localize("Info.SkipFor", cost.Type.ToString(), cost.Amount);
		}
	}
}
