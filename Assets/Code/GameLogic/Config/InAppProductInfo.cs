using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class InAppProductInfo : InAppProductInfoBase {
		[MetaMember(1)] public List<ResourceInfo> Resources { get; private set; }
		[MetaMember(2)] public List<ItemCountInfo> Items { get; private set; }
		[MetaMember(3)] public string Icon { get; private set; }
		[MetaMember(4)] public IslandTypeId Island { get; private set; }
		[MetaMember(5)] public string LockArea { get; private set; }
		[MetaMember(6)] public VipPassId VipPassId { get; private set; }
		[MetaMember(7)] public MetaDuration VipPassDuration { get; private set; }
	}
}
