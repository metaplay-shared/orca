using System;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ResourceTriggerInfo : IGameConfigData<ResourceTriggerId> {
		[MetaMember(1)] public CurrencyTypeId Currency { get; private set; }
		[MetaMember(2)] public int Amount { get; private set; }
		[MetaMember(3)] public TriggerId Trigger { get; private set; }
		[MetaMember(4)] public IslandTypeId Island { get; private set; }

		public ResourceTriggerId ConfigKey => new(Island, Currency, Amount);
	}

	[MetaSerializable]
	public class ResourceTriggerId : IEquatable<ResourceTriggerId> {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public CurrencyTypeId Currency { get; private set; }
		[MetaMember(3)] public int Amount { get; private set; }

		public ResourceTriggerId() {}

		public ResourceTriggerId(IslandTypeId island, CurrencyTypeId currency, int amount) {
			Island = island;
			Currency = currency;
			Amount = amount;
		}

		public bool Equals(ResourceTriggerId other) {
			if (ReferenceEquals(null, other)) { return false; }
			if (ReferenceEquals(this, other)) { return true; }
			return Equals(Island, other.Island) && Currency == other.Currency && Amount == other.Amount;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) { return false; }
			if (ReferenceEquals(this, obj)) { return true; }
			if (obj.GetType() != this.GetType()) { return false; }
			return Equals((ResourceTriggerId) obj);
		}

		public override int GetHashCode() {
			return HashCode.Combine(Island, Currency, Amount);
		}

		public override string ToString() {
			return Island + ":" + Currency + ":" + Amount;
		}
	}
}
