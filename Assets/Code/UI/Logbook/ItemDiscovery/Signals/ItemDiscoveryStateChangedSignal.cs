using Game.Logic;

namespace Code.UI.ItemDiscovery.Signals {
	public class ItemDiscoveryStateChangedSignal {
		public LevelId<ChainTypeId> ChainId { get; private set; }

		public ItemDiscoveryStateChangedSignal(LevelId<ChainTypeId> chainId) {
			ChainId = chainId;
		}
	}
}
