using Code.Purchasing;
using System.Threading;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Tasks {
	public class SkipTimerButton : ButtonHelper {
		[SerializeField] private CurrencyLabel Cost;

		[Inject] private IPurchasingFlowController purchasingFlowController;

		private HeroTypeId hero;

		protected override void OnClick() {
			Cost cost = MetaplayClient.PlayerModel.SkipHeroTaskTimerCost(hero);

			purchasingFlowController.TrySpendGemsAsync(cost.Amount, CancellationToken.None)
				.ContinueWith(
					success => {
						if (success) {
							MetaplayClient.PlayerContext.ExecuteAction(new PlayerSkipHeroTaskTimer(hero));
						}
					}
				).Forget();
		}

		public void Setup(HeroTypeId heroType) {
			hero = heroType;
			Cost cost = MetaplayClient.PlayerModel.SkipHeroTaskTimerCost(hero);
			Cost.Set(cost.Type, cost.Amount);
		}

		private float nextUpdate = 0;
		private void Update() {
			if (Time.time >= nextUpdate) {
				Cost cost = MetaplayClient.PlayerModel.SkipHeroTaskTimerCost(hero);
				Cost.Set(cost.Type, cost.Amount);
				nextUpdate = Time.time + 1;
			}
		}
	}
}
