using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;

namespace Code.UI.Merge.Hero {
	public class HeroTaskTimer : UpdateLabel {
		protected override string SourceText {
			get {
				if (heroModel == null) {
					return "";
				}

				if (heroModel.CurrentTask == null) {
					return "";
				}

				if (heroModel.CurrentTask.State != HeroTaskState.Fulfilled) {
					return "";
				}

				var timeRemaining = (heroModel.CurrentTask.FinishedAt - MetaplayClient.PlayerModel.CurrentTime)
					.ToSimplifiedString();

				return timeRemaining;
			}
		}
		protected override float UpdateIntervalSeconds => 1.0f;

		private HeroModel heroModel;

		public void Setup(HeroModel hero) {
			heroModel = hero;
		}
	}
}
