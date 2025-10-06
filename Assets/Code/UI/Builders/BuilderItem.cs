using Code.Purchasing;
using System;
using System.Linq;
using System.Threading;
using Code.UI.Application;
using Code.UI.Core;
using Code.UI.Hud.Signals;
using Code.UI.MergeBase;
using Code.UI.Shop;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Builders {
	public class BuilderItem : MonoBehaviour {
		[SerializeField] private TMP_Text NumberText;
		[SerializeField] private TMP_Text LocationText;
		[SerializeField] private BuildTimeRemainingLabel TimeRemainingLabelText;
		[SerializeField] private TMP_Text TemporaryBuilderExpirationTimeText;
		[SerializeField] private Button SkipTimerButton;
		[SerializeField] private Button GoToButton;
		[SerializeField] private CurrencyLabel SkipLabel;

		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IPurchasingFlowController purchasingFlowController;
		[Inject] private UIController uiController;
		[Inject] private SignalBus signalBus;
		[Inject] private MergeBoardRoot mergeBoardRoot;
		[Inject] private IUIRootController uiRootController;

		private BuilderModel builderModel;
		private Action goToCallback;

		private Cost SkipCost => MetaplayClient.PlayerModel.SkipBuilderTimerCost(builderModel.Id);

		public void Setup(BuilderModel builder, Action goToCallback) {
			signalBus.Subscribe<BuildersChangedSignal>(Refresh);
			this.goToCallback = goToCallback;
			builderModel = builder;
			NumberText.text = builder.Id.ToString();

			Refresh();
		}

		private float nextUpdate = 0;

		private void Update() {
			if (Time.time >= nextUpdate) {
				if (builderModel?.IsFree == true) {
					return;
				}

				SkipLabel.Set(SkipCost.Type, SkipCost.Amount);
				nextUpdate = Time.time + 1;
			}
		}

		private void OnDestroy() {
			signalBus.Unsubscribe<BuildersChangedSignal>(Refresh);
		}

		private void Refresh() {
			if (builderModel.IsFree) {
				LocationText.text = Localizer.Localize("Info.BuilderAvailable");
				GoToButton.gameObject.SetActive(false);
				SkipTimerButton.gameObject.SetActive(false);
				TimeRemainingLabelText.gameObject.SetActive(false);
				TemporaryBuilderExpirationTimeText.gameObject.SetActive(false);
				return;
			}

			GoToButton.gameObject.SetActive(true);
			SkipTimerButton.gameObject.SetActive(true);
			TimeRemainingLabelText.gameObject.SetActive(true);
			TemporaryBuilderExpirationTimeText.gameObject.SetActive(true);

			LocationText.text = builderModel.Island.Localize();
			TimeRemainingLabelText.Setup(builderModel);

			if (builderModel.ExpiresAt != MetaTime.Epoch) {
				TemporaryBuilderExpirationTimeText.text =
					(builderModel.ExpiresAt - MetaplayClient.PlayerModel.CurrentTime).ToSimplifiedString();
			} else {
				TemporaryBuilderExpirationTimeText.gameObject.SetActive(false);
			}

			SkipLabel.Set(SkipCost.Type, SkipCost.Amount);
		}

		public void OnSkipClicked() {
			Cost cost = SkipCost;
			ItemModel mergeItem = MetaplayClient.PlayerModel.Islands[builderModel.Island].MergeBoard.Items
				.FirstOrDefault(i => i.UsedBuilderId == builderModel.Id);

			if (mergeItem == null) {
				return;
			}

			if (cost.Type == CurrencyTypeId.Gems) {
				purchasingFlowController.TrySpendGemsAsync(cost.Amount, CancellationToken.None)
					.ContinueWith(
						success => {
							if (success) {
								MetaplayClient.PlayerContext.ExecuteAction(
									new PlayerSkipBuilderTimer(builderModel.Island, mergeItem.X, mergeItem.Y)
								);
							}
						}
					).Forget();
			} else {
				if (MetaplayClient.PlayerModel.Wallet.EnoughCurrency(cost.Type, cost.Amount)) {
					MetaplayClient.PlayerContext.ExecuteAction(
						new PlayerSkipBuilderTimer(builderModel.Island, mergeItem.X, mergeItem.Y)
					);
				} else {
					uiRootController.ShowUI<ShopUIRoot, ShopUIHandle>(
						new ShopUIHandle(new ShopUIHandle.MarketNavigationPayload(ShopCategoryId.Gold)),
						CancellationToken.None
					);
				}
			}
		}

		public void OnGoToClicked() {
			goToCallback.Invoke();
			Animate(CancellationToken.None).Forget();
		}

		private async UniTask Animate(CancellationToken ct) {
			IslandTypeId islandTypeId = builderModel.Island;
			Debug.Log($"Animating to {islandTypeId}");
			await uiController.GoToIslandAsync(islandTypeId, ct);

			if (applicationInfo.ActiveIsland.Value != null) {
				ItemModel builderTargetItem = MetaplayClient.PlayerModel.Islands[islandTypeId]
					.MergeBoard.Items.Single(i => i.BuilderId == builderModel.Id);
				MergeTile builderTargetMergeTile = mergeBoardRoot.TileAt(builderTargetItem.X, builderTargetItem.Y);
				mergeBoardRoot.Select(builderTargetMergeTile.Item, false);
			}
		}
	}
}
