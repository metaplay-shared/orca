using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;

namespace Code.UI.Tasks {
	public class ClaimRewardButton : ButtonHelper {
		private HeroTypeId hero;

		public void Setup(HeroTypeId heroType) {
			hero = heroType;
		}

		protected override void OnClick() {
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerClaimHeroTaskRewards(hero));
		}
	}
}
