using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class IdGenerator {
		[MetaMember(1)] public int NextId { get; private set; } = 0;

		public IdGenerator() { }

		public IdGenerator(int initialValue) {
			NextId = initialValue;
		}

		public int GetNextId() {
			NextId += 1;
			return NextId;
		}
	}
}
