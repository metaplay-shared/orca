using Game.Logic;

namespace Code.UI.Application.Signals {
	public class FeatureUnlockedSignal {
		public FeatureTypeId Feature { get; }

		public FeatureUnlockedSignal(FeatureTypeId feature) {
			Feature = feature;
		}
	}
}
