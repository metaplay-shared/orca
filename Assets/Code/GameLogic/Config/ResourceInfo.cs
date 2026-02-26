using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ResourceInfo {
		[MetaMember(1)] public CurrencyTypeId Type { get; private set; }
		[MetaMember(2)] public int Amount { get; private set; }

		public ResourceInfo() { }

		public ResourceInfo(CurrencyTypeId type, int amount) {
			Type = type;
			Amount = amount;
		}

		public override string ToString() {
			return $"{Amount}*{Type.Value}";
		}
	}
}
