using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ItemCountInfo {
		[MetaMember(1)] public ChainTypeId Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }
		[MetaMember(3)] public int Count { get; private set; }

		public LevelId<ChainTypeId> ChainId => new(Type, Level);

		public ItemCountInfo() {}

		public ItemCountInfo(ChainTypeId type, int level, int count) {
			Type = type;
			Level = level;
			Count = count;
		}

		public override string ToString() {
			return $"{Count}*{Type.Value}:{Level}";
		}
	}
}
