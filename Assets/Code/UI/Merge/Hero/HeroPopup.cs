using System.Collections.Generic;
using System.Threading;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.Hero {
	public class HeroPopupPayload : UIHandleBase {
		public ChainTypeId Building { get; }

		public HeroPopupPayload(ChainTypeId building) {
			Building = building;
		}
	}

	public class HeroPopup : UIRootBase<HeroPopupPayload> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private HeroTask HeroTaskTemplate;
		[SerializeField] private GameObject LockedHeroTemplate;
		[SerializeField] private Transform HeroContainer;
		[SerializeField] private ScrollRect ScrollRect;
		[SerializeField] private TMP_Text Title;
		[SerializeField] private TMP_Text Subtitle;

		[Inject] private DiContainer container;
		[Inject] private SignalBus signalBus;

		protected override void Init() {
			InstantiateHeroes();
			ScrollRect.verticalNormalizedPosition = 1f;
			Title.text = Localizer.Localize($"HeroPopup.{UIHandle.Building.Value}.Title");
			Subtitle.text = Localizer.Localize($"HeroPopup.{UIHandle.Building.Value}.Subtitle");
		}

		protected override async UniTask Idle(CancellationToken ct) {
			try {
				signalBus.Subscribe<HeroAssignedToBuildingSignal>(UpdateHeroes);
				await UniTask.WhenAny(
					CloseButton.OnClickAsync(ct),
					OnBackgroundClickAsync(ct)
				);
			} finally {
				signalBus.TryUnsubscribe<HeroAssignedToBuildingSignal>(UpdateHeroes);
			}
		}

		private void UpdateHeroes(HeroAssignedToBuildingSignal signal) {
			if (signal.SourceBuilding == UIHandle.Building ||
				signal.TargetBuilding == UIHandle.Building) {
				foreach (Transform child in transform) {
					Destroy(child.gameObject);
				}
			}

			InstantiateHeroes();
		}

		private void InstantiateHeroes() {
			List<HeroModel> heroes = MetaplayClient.PlayerModel.Heroes.HeroesInBuilding(UIHandle.Building);
			foreach (HeroModel hero in heroes) {
				SetupHero(hero);
			}

			int totalCount = MetaplayClient.PlayerModel.GameConfig.Global.MaxHeroesInBuilding;
			for (int i = heroes.Count; i < totalCount; i++) {
				container.InstantiatePrefab(LockedHeroTemplate, HeroContainer);
			}
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}

		private void SetupHero(HeroModel hero) {
			HeroTask heroTask = container.InstantiatePrefabForComponent<HeroTask>(HeroTaskTemplate, HeroContainer);
			heroTask.Setup(hero);
			heroTask.ClaimRewardButton.GetComponent<Button>().onClick.AddListener(CloseButton.onClick.Invoke);
		}
	}
}
