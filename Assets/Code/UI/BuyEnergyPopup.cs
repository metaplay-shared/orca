using System.Threading;
using Code.Purchasing;
using Code.UI.Application;
using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI {
	public class BuyEnergyPopupPayload : UIHandleBase { }

	public class BuyEnergyPopup : UIRootBase<BuyEnergyPopupPayload> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private Button BuyButton;
		[SerializeField] private TMP_Text MessageText;
		[SerializeField] private CurrencyLabel ButtonLabel;
		[SerializeField] private Button GoToEnergyIslandButton;

		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IPurchasingFlowController purchasingFlowController;
		[Inject] private UIController uiController;

		private string OutOfEnergy => "Out of energy";

		protected override void Init() {
			MessageText.text = OutOfEnergy;
			BuyButton.onClick.AddListener(TryBuyEnergy);
			var cost = MetaplayClient.PlayerModel.Merge.EnergyFill.EnergyCost(MetaplayClient.PlayerModel.GameConfig);
			ButtonLabel.Set(cost.CurrencyType, cost.Cost);
			IslandModel energyIsland = MetaplayClient.PlayerModel.Islands[IslandTypeId.EnergyIsland];
			bool isEnergyIslandUnlocked = energyIsland.State == IslandState.Open;
			GoToEnergyIslandButton.gameObject.SetActive(isEnergyIslandUnlocked);
		}

		protected override async UniTask Idle(CancellationToken ct) {
			GoToEnergyIslandButton
				.OnClickAsAsyncEnumerable(ct)
				.ForEachAwaitAsync(
					_ => uiController.GoToIslandAsync(IslandTypeId.EnergyIsland, CancellationToken.None),
					ct
				).Forget();
			await UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct),
				GoToEnergyIslandButton.OnClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}

		private void TryBuyEnergy() {
			EnergyCostInfo cost =
				MetaplayClient.PlayerModel.Merge.EnergyFill.EnergyCost(MetaplayClient.PlayerModel.GameConfig);

			purchasingFlowController.TrySpendGemsAsync(cost.Cost, CancellationToken.None)
				.ContinueWith(
					success => {
						if (success) {
							IslandTypeId island = applicationInfo.ActiveIsland.Value == null
								? IslandTypeId.None
								: applicationInfo.ActiveIsland.Value;

							MetaplayClient.PlayerContext.ExecuteAction(new PlayerFillEnergy(island));
						}
					}
				).Forget();

			CloseButton.onClick.Invoke();
		}
	}
}
