using System.Threading;
using Code.Purchasing;
using Code.UI.Application;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class CreatorTimerAddOn : ItemAddOn {

		[SerializeField] private TimerIcon Timer;
		[SerializeField] private TimerBubble TimerBubble;

		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IPurchasingFlowController purchasingFlowController;

		private Cost SkipCost =>
			ItemModel.SkipCreatorTimerCost(
				MetaplayClient.PlayerModel.GameConfig,
				MetaplayClient.PlayerModel.CurrentTime
			);

		protected override void Setup() {
			OnStateChanged();
		}

		public override void OnSelected() {
			if (ItemModel.HasTimer) {
				TimerBubble.Show(ItemOverlayLayer, gameObject.transform);
			}
		}

		public override void OnDeselected() {
			TimerBubble.Hide();
		}

		public override void OnBeginDrag() {
			TimerBubble.Hide();
		}

		public override void OnDestroySelf() {
			if (TimerBubble != null) {
				TimerBubble.Hide();
			}
		}

		public override void OnStateChanged() {
			Timer.gameObject.SetActive(ItemModel.HasTimer);
		}

		private void Awake() {
			TimerBubble.AddClickListener(OnSkipClicked);
		}

		private float nextUpdate = 0;
		private void Update() {
			if (TimerBubble == null) {
				return;
			}

			if (!ItemModel.HasTimer) {
				TimerBubble.Hide();
				return;
			}

			if (Time.time >= nextUpdate) {
				Cost cost = SkipCost;
				MetaDuration timeLeft = ItemModel.Creator.TimeToFill(MetaplayClient.PlayerModel.CurrentTime);
				var timeStr = timeLeft.ToSimplifiedString();
				TimerBubble.SetCostAndTimer(cost.Type, cost.Amount, timeStr);

				Timer.UpdateFill(
					ItemModel.Creator.TimeToFill(MetaplayClient.PlayerModel.CurrentTime),
					ItemModel.Creator.Info.FullRechargeTime
				);
				nextUpdate = Time.time + 1;
			}
		}

		private void OnSkipClicked() {
			purchasingFlowController.TrySpendGemsAsync(SkipCost.Amount, CancellationToken.None)
				.ContinueWith(
					success => {
						if (success) {
							MetaplayClient.PlayerContext.ExecuteAction(
								new PlayerSkipCreatorTimer(applicationInfo.ActiveIsland.Value, ItemModel.X, ItemModel.Y)
							);
						}
					}
				)
				.Forget();
		}
	}
}
