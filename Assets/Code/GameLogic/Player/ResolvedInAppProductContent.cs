using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializableDerived(1)]
	public class ResolvedInAppProductContent : ResolvedPurchaseContentBase {
		[MetaMember(1)] public List<ResourceInfo> Resources { get; private set; }
		[MetaMember(2)] public List<ItemCountInfo> Items { get; private set; }

		ResolvedInAppProductContent() { }

		public ResolvedInAppProductContent(List<ResourceInfo> resources, List<ItemCountInfo> items) {
			Resources = resources;
			Items = items;
		}
	}

	[MetaSerializableDerived(2)]
	public class ResolvedInAppIslandLockArea : ResolvedPurchaseContentBase {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public string LockArea { get; private set; }

		ResolvedInAppIslandLockArea() {}

		public ResolvedInAppIslandLockArea(IslandTypeId island, string lockArea) {
			Island = island;
			LockArea = lockArea;
		}
	}

	[MetaSerializableDerived(3)]
	public class ResolvedInAppVipPass : ResolvedPurchaseContentBase {
		[MetaMember(1)] public VipPassId VipPass { get; private set; }
		[MetaMember(2)] public MetaDuration Duration { get; private set; }

		ResolvedInAppVipPass() {}

		public ResolvedInAppVipPass(VipPassId vipPass, MetaDuration duration) {
			VipPass = vipPass;
			Duration = duration;
		}
	}
}
