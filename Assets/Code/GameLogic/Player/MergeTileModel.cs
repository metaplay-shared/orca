using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	public class MergeTileModel {
		public ItemModel Item { get; set; }
		public TileType Type { get; private set; }

		public bool IsFree => Item == null && Type == TileType.Ground;
		public bool HasItem => Item != null;

		public MergeTileModel(TileType type) {
			Type = type;
		}
	}
}
