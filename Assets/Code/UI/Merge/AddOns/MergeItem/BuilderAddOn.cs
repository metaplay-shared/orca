using Code.Purchasing;
using System.Threading;
using Code.UI.Application;
using Code.UI.Core;
using Code.UI.InfoMessage.Signals;
using Code.UI.Shop;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class BuilderAddOn : ItemAddOn {
		[SerializeField] private Image Art;
		[SerializeField] private TimerIcon Timer;
		[SerializeField] private TimerBubble TimerBubble;

		[SerializeField] private RectTransform BuildBubble;
		[SerializeField] private TMP_Text BuildTimeText;
		[SerializeField] private Button BuildButton;
		[SerializeField] private GameObject BuilderNeededIndicator;

		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IPurchasingFlowController purchasingFlowController;
		[Inject] private IUIRootController uiRootController;

		private bool selected;
		private bool shown;

		private Cost SkipCost => MetaplayClient.PlayerModel.SkipBuilderTimerCost(ItemModel.UsedBuilderId);

		protected override void Setup() {
			F64 discountFactor = MetaplayClient.PlayerModel.BuilderTimerFactor();
			MetaDuration buildTime =
				MetaDuration.FromSeconds(F64.CeilToInt(discountFactor * ItemModel.Info.BuildTime.ToSecondsF64()));
			BuildTimeText.text = buildTime.ToSimplifiedString();
		}

		public override void OnStateChanged() {
			if (ItemModel.BuildState == ItemBuildState.Complete) {
				Art.color = Color.white;
				BuilderNeededIndicator.SetActive(false);
			} else {
				ColorInfo color = MetaplayClient.PlayerModel.GameConfig.Client.InactiveItemColor;
				Art.color = new Color(color.Red, color.Green, color.Blue, color.Alpha);
				BuilderNeededIndicator.SetActive(true);
			}

			Timer.gameObject.SetActive(ItemModel.IsUsingBuilder);
			BuildBubble.gameObject.SetActive(false);
			if (!shown &&
				selected &&
				ItemModel.IsUsingBuilder) {
				UpdateTimerBubbleValues();
				TimerBubble.Show(ItemOverlayLayer, gameObject.transform);
				shown = true;
			} else {
				TimerBubble.Hide();
			}
		}

		public override void OnSelected() {
			if (ItemModel.BuildState == ItemBuildState.NotStarted) {
				BuildBubble.SetParent(gameObject.transform, false);
				BuildBubble.anchoredPosition = Vector3.zero;
				BuildBubble.SetParent(ItemOverlayLayer, true);
				BuildBubble.localScale = Vector3.one;
				BuildBubble.gameObject.SetActive(true);

				UpdateInitialBuildTime();
			} else if (ItemModel.IsUsingBuilder) {
				TimerBubble.Show(ItemOverlayLayer, gameObject.transform);
			}

			selected = true;
		}

		public override void OnDeselected() {
			BuildBubble.gameObject.SetActive(false);
			TimerBubble.Hide();

			selected = false;
		}

		public override void OnBeginDrag() {
			BuildBubble.gameObject.SetActive(false);
			TimerBubble.Hide();
		}

		public override void OnDestroySelf() {
			if (BuildBubble != null) {
				BuildBubble.gameObject.SetActive(false);
			}

			if (TimerBubble != null) {
				TimerBubble.Hide();
			}
		}

		private void Awake() {
			TimerBubble.AddClickListener(OnSkipClicked);
			BuildButton.onClick.AddListener(OnBuildClicked);
		}

		private float nextUpdate = 0;

		private void Update() {
			if (!ItemModel.IsUsingBuilder) {
				TimerBubble.Hide();
			}

			if (Time.time >= nextUpdate) {
				if (TimerBubble != null) {
					UpdateTimerBubbleValues();
				}

				if (BuildBubble != null) {
					UpdateInitialBuildTime();
				}

				nextUpdate = Time.time + 1;
			}
		}

		private void UpdateTimerBubbleValues() {
			MetaDuration timeLeft = MetaplayClient.PlayerModel.BuildTimeLeft(ItemModel.UsedBuilderId);
			Timer.UpdateFill(timeLeft, MetaplayClient.PlayerModel.TotalBuildTime(ItemModel.UsedBuilderId));

			Cost cost = SkipCost;
			var timeStr = timeLeft.ToSimplifiedString();
			TimerBubble.SetCostAndTimer(cost.Type, cost.Amount, timeStr);
		}

		private void UpdateInitialBuildTime() {
			F64 discountFactor = MetaplayClient.PlayerModel.BuilderTimerFactor();
			MetaDuration buildTime =
				MetaDuration.FromSeconds(F64.CeilToInt(discountFactor * ItemModel.Info.BuildTime.ToSecondsF64()));
			BuildTimeText.text = buildTime.ToSimplifiedString();
		}

		private void OnBuildClicked() {
			if (MetaplayClient.PlayerModel.Builders.Free > 0) {
				MetaplayClient.PlayerContext.ExecuteAction(
					new PlayerUseBuilder(applicationInfo.ActiveIsland.Value, ItemModel.X, ItemModel.Y)
				);
				// Prevents "0s FREE" from showing for a split second after clicking "Build".
				UpdateTimerBubbleValues();
			} else {
				SignalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.NoBuildersAvailable")));
			}
		}

		private void OnSkipClicked() {
			if (SkipCost.Type == CurrencyTypeId.Gems) {
				purchasingFlowController.TrySpendGemsAsync(SkipCost.Amount, CancellationToken.None)
					.ContinueWith(
						success => {
							if (success) {
								MetaplayClient.PlayerContext.ExecuteAction(
									new PlayerSkipBuilderTimer(applicationInfo.ActiveIsland.Value, ItemModel.X, ItemModel.Y)
								);
							}
						}
					).Forget();
			} else {
				if (MetaplayClient.PlayerModel.Wallet.EnoughCurrency(SkipCost.Type, SkipCost.Amount)) {
					MetaplayClient.PlayerContext.ExecuteAction(
						new PlayerSkipBuilderTimer(applicationInfo.ActiveIsland.Value, ItemModel.X, ItemModel.Y)
					);
				} else {
					uiRootController.ShowUI<ShopUIRoot, ShopUIHandle>(
						new ShopUIHandle(new ShopUIHandle.MarketNavigationPayload(ShopCategoryId.Gold)),
						CancellationToken.None
					);
				}
			}
		}
	}
}
