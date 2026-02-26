using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class EnergyCostInfo : IGameConfigData<int> {
		[MetaMember(1)] public int Index { get; private set; }
		[MetaMember(2)] public CurrencyTypeId CurrencyType { get; private set; }
		[MetaMember(3)] public int Cost { get; private set; }

		public int ConfigKey => Index;
	}
}
