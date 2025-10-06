using System;
using Metaplay.Core.Config;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class LockAreaInfo : IGameConfigData<LockAreaId> {
		[MetaMember(1)] public string Index { get; private set; }
		[MetaMember(2)] public IslandTypeId IslandId { get; private set; }
		[MetaMember(3)] public int PlayerLevel { get; private set; }
		[MetaMember(4)] public HeroTypeId Hero { get; private set; }
		[MetaMember(5)] public int HeroLevel { get; private set; }
		[MetaMember(6)] public ResourceInfo UnlockCost { get; private set; }
		[MetaMember(7)] public InAppProductId UnlockProduct { get; private set; }
		[MetaMember(8)] public F64 LockX { get; private set; }
		[MetaMember(9)] public F64 LockY { get; private set; }
		[MetaMember(10)] public bool Transparent { get; private set; }
		[MetaMember(11)] public string Dependency { get; private set; }

		public char AreaIndex => Index[0];

		public LockAreaId ConfigKey => new LockAreaId(IslandId, Index);
	}

	[MetaSerializable]
	public class LockAreaId : IEquatable<LockAreaId> {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public string Index { get; private set; }

		public LockAreaId() {}

		public LockAreaId(IslandTypeId island, string index) {
			Island = island;
			Index = index;
		}

		public bool Equals(LockAreaId other) {
			if (ReferenceEquals(null, other)) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			return Equals(Island, other.Island) && Index == other.Index;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) { return false; }
			if (ReferenceEquals(this, obj)) { return true; }
			if (obj.GetType() != this.GetType()) { return false; }
			return Equals((LockAreaId) obj);
		}

		public override int GetHashCode() {
			return HashCode.Combine(Island, Index);
		}

		public override string ToString() {
			return Island + ":" + Index;
		}
	}
}
