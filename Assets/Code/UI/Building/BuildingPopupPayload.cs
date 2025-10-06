using Code.UI.Core;
using Game.Logic;

namespace Code.UI.Building {
	public class BuildingPopupPayload : UIHandleBase {
		public ChainTypeId ItemType { get; private set; }
		public ChainTypeId BuildingType { get; private set; }

		public BuildingPopupPayload(ChainTypeId itemType, ChainTypeId buildingType) {
			ItemType = itemType;
			BuildingType = buildingType;
		}
	}
}
