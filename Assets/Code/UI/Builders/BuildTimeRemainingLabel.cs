using Code.UI.Utils;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;

namespace Code.UI.Builders {
	public class BuildTimeRemainingLabel : UpdateLabel {
		protected override string SourceText =>
			builderId > 0 && MetaplayClient.PlayerModel.BuildTimeLeft(builderId) > MetaDuration.Zero
				? MetaplayClient.PlayerModel.BuildTimeLeft(builderId).ToSimplifiedString()
				: "";
		protected override float UpdateIntervalSeconds => 1.0f;

		private int builderId;

		public void Setup(BuilderModel builderModel) {
			builderId = builderModel?.Id ?? 0;
		}
	}
}
