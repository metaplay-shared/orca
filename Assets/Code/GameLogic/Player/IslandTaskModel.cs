using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class IslandTaskModel {
		[MetaMember(1)] public IslandTaskInfo Info { get; private set; }
		[MetaMember(2)] public bool Enabled { get; set; }

		public IslandTaskModel() { }

		public IslandTaskModel(IslandTaskInfo info) {
			Info = info;
		}

		public bool IsItemUsed(LevelId<ChainTypeId> id) {
			if (!Enabled) {
				return false;
			}
			foreach (ItemCountInfo item in Info.Items) {
				if (item.Type == id.Type && item.Level == id.Level) {
					return true;
				}
			}

			return false;
		}
		
		public int GetUsedItemLevel(ChainTypeId id) {
			if (!Enabled) {
				return -1;
			}
			foreach (ItemCountInfo item in Info.Items) {
				if (item.Type == id) {
					return item.Level;
				}
			}

			return -1;
		}
	}
}
