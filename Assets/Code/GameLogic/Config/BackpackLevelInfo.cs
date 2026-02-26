using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class BackpackLevelInfo : IGameConfigData<int> {
		[MetaMember(1)] public int Level { get; private set; }
		[MetaMember(2)] public int Slots { get; private set; }
		[MetaMember(3)] public ResourceInfo UnlockCost { get; private set; }

		public int ConfigKey => Level;
	}
}
