using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ShopInfo : GameConfigKeyValue<ShopInfo> {
		[MetaMember(1)] public ResourceInfo RefreshCost { get; private set; }
		[MetaMember(2)] public MetaDuration RefreshInterval { get; private set; }
		[MetaMember(3)] public List<ShopCategoryId> CategoryOrder { get; private set; }
		[MetaMember(4)] public TriggerId FirstOpenTrigger { get; private set; }
		[MetaMember(5)] public TriggerId FirstCloseTrigger { get; private set; }
	}
}
