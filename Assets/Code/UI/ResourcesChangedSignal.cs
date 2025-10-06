using Game.Logic;

namespace Code.UI {
	public class ResourcesChangedSignal {
		public CurrencyTypeId ResourceType { get; }
		public int Diff { get; }

		public ResourcesChangedSignal(CurrencyTypeId resourceType, int diff) {
			ResourceType = resourceType;
			Diff = diff;
		}
	}
}
