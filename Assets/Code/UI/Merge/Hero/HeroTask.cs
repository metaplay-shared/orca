using System;
using System.Linq;
using Code.UI.AssetManagement;
using Code.UI.RequirementsDisplay;
using Code.UI.Tasks;
using Code.UI.Tasks.Hero;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.Hero {
	public class HeroTask : MonoBehaviour {
		[SerializeField] private TMP_Text HeroName;
		[SerializeField] private TMP_Text HeroLevel;
		[SerializeField] private Image HeroImage;

		[SerializeField] private GameObject ClaimView;
		[SerializeField] private GameObject CompleteView;
		[SerializeField] private GameObject WaitView;

		[SerializeField] private CompleteTaskButton CompleteTaskButton;

		[SerializeField] private SkipTimerButton SkipTimerButton;
		[SerializeField] private HeroTaskTimer HeroTaskTimer;

		[SerializeField] public ClaimRewardButton ClaimRewardButton;

		[SerializeField] private ResourceRequirements ResourceRequirements;

		[SerializeField] private RectTransform PreTaskRewardContainer;
		[SerializeField] private RectTransform PostTaskRewardContainer;

		[SerializeField] private HeroTaskReward HeroTaskRewardTemplate;

		[Inject] private DiContainer container;
		[Inject] private AddressableManager addressableManager;
		[Inject] private SignalBus signalBus;

		private HeroModel hero;

		private bool CanClaim => hero.CurrentTask.State == HeroTaskState.Finished;
		private bool CanSkip => hero.CurrentTask.State == HeroTaskState.Fulfilled;
		private bool CanComplete => hero.CurrentTask.State == HeroTaskState.Created;

		public void UpdateDetails() {
			ClaimView.gameObject.SetActive(CanClaim);
			CompleteView.gameObject.SetActive(CanComplete);
			WaitView.gameObject.SetActive(CanSkip);

			var requirementItems = hero.CurrentTask.Info.Resources.Select(
				r => new RequirementResourceItem() {
					RequiredAmount = r.Amount,
					TypeName = r.Type.Value
				}
			).ToArray();

			ResourceRequirements.SetupAsync(requirementItems).Forget();
		}

		public void Setup(HeroModel hero) {
			this.hero = hero;
			HeroName.text = hero.Info.Type.Localize();
			HeroLevel.text = "lvl. " + hero.Level.Level;
			HeroImage.sprite = addressableManager.Get<Sprite>($"Heroes/{hero.Info.Type}.png");

			HeroTaskTimer.Setup(hero);

			container.Inject(ResourceRequirements);
			container.Inject(SkipTimerButton);

			CompleteTaskButton.Setup(hero.Info.Type);
			SkipTimerButton.Setup(hero.Info.Type);
			ClaimRewardButton.Setup(hero.Info.Type);

			UpdateDetails();
			SetupRewards();
		}

		private void OnEnable() {
			signalBus.Subscribe<HeroTaskModifiedSignal>(UpdateTask);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<HeroTaskModifiedSignal>(UpdateTask);
		}

		private void UpdateTask(HeroTaskModifiedSignal signal) {
			if (signal.HeroType == hero.Info.Type) {
				UpdateDetails();
			}
		}

		private void SetupRewards() {
			foreach (var reward in hero.CurrentTask.Info.Rewards) {
				SpawnRewardItem(reward, PreTaskRewardContainer);
				SpawnRewardItem(reward, PostTaskRewardContainer);
			}

			void SpawnRewardItem(ItemCountInfo reward, RectTransform parent) {
				HeroTaskReward taskReward =
					container.InstantiatePrefabForComponent<HeroTaskReward>(HeroTaskRewardTemplate, parent);
				taskReward.Setup(reward);
			}
		}
	}
}
