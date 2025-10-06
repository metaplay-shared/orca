using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class DailyTaskInfo {
		[MetaMember(1)] public DailyTaskTypeId Type { get; private set; }
		[MetaMember(2)] public int Amount { get; private set; }
		[MetaMember(3)] public ResourceInfo Reward { get; private set; }
		[MetaMember(4)] public string Icon { get; private set; }

		public DailyTaskInfo() { }

		public DailyTaskInfo(DailyTaskTypeId type, int amount, CurrencyTypeId currencyTypeId, int currencyAmount, string icon) {
			Type = type;
			Amount = amount;
			Reward = new ResourceInfo(currencyTypeId, currencyAmount);
			Icon = icon;
		}

		public override string ToString() {
			return $"{Amount} x {Type}, reward {Reward}, icon {Icon}";
		}
	}
}
