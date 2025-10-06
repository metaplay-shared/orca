using Code.Purchasing;
using System.Threading;
using Code.UI.Application;
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

namespace Code.UI.Merge.AddOns.MergeItem {
	public class BubbleAddOn : ItemAddOn {
		[SerializeField] private GameObject BubbleIcon;
		[SerializeField] private TimerIcon Timer;
		[SerializeField] private RectTransform ClaimBubble;
		[SerializeField] private CurrencyLabel ClaimCostLabel;
		[SerializeField] private TMP_Text TimeLeftLabel;

		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IPurchasingFlowController purchasingFlowController;

		public override bool IsActive => ItemModel?.Bubble == true;

		protected override void Setup() {
			if (!IsActive) {
				return;
			}

			Timer.gameObject.SetActive(true);
			BubbleIcon.SetActive(true);
			int cost = ItemModel.Info.BubblePrice;
			ClaimCostLabel.Set(CurrencyTypeId.Gems, cost);
			UpdateClaimBubble();
		}

		public override void OnStateChanged() {
			if (!IsActive) {
				Timer.gameObject.SetActive(false);
				BubbleIcon.SetActive(false);
				HideBubble(ClaimBubble);
			}
		}

		public override void OnSelected() {
			ShowBubble(ClaimBubble);
		}

		public override void OnDeselected() {
			HideBubble(ClaimBubble);
		}

		public override void OnBeginDrag() {
			HideBubble(ClaimBubble);
		}

		public override void OnDestroySelf() {
			if (ClaimBubble != null) {
				HideBubble(ClaimBubble);
			}
		}

		private float nextUpdate = 0;

		private void Update() {
			if (!IsActive) {
				return;
			}

			if (Time.time >= nextUpdate) {
				Timer.UpdateFill(
					ItemModel.OpenTimeLeft(MetaplayClient.PlayerModel.CurrentTime),
					MetaplayClient.PlayerModel.GameConfig.Global.BubbleTtl
				);
				UpdateClaimBubble();

				nextUpdate = Time.time + 1;
			}
		}

		private void UpdateClaimBubble() {
			TimeLeftLabel.text = ItemModel.OpenTimeLeft(MetaplayClient.PlayerModel.CurrentTime).ToSimplifiedString();
		}

		public void OnClaimClicked() {
			int cost = ItemModel.Info.BubblePrice;

			purchasingFlowController.TrySpendGemsAsync(cost, CancellationToken.None)
				.ContinueWith(
					success => {
						if (success) {
							MetaplayClient.PlayerContext.ExecuteAction(
								new PlayerOpenBubble(applicationInfo.ActiveIsland.Value, ItemModel.X, ItemModel.Y)
							);
						}
					}
				).Forget();
		}
	}
}
