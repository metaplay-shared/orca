using System.Collections.Generic;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class BackpackModel {
		[MetaMember(1)] public BackpackLevelInfo Info { get; private set; }
		[MetaMember(2)] public List<ItemModel> Items { get; private set; }

		public bool IsFull => Items.Count >= Info.Slots;

		public BackpackModel() { }

		public BackpackModel(SharedGameConfig gameConfig) {
			Info = gameConfig.BackpackLevels[1];
			Items = new List<ItemModel>();
		}

		public void Upgrade(SharedGameConfig gameConfig) {
			Info = gameConfig.BackpackLevels[Info.Level + 1];
		}
	}
}
