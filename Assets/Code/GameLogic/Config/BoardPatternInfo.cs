using System.Runtime.Serialization;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class BoardPatternInfo {
		private const char LINE_SEPARATOR = '|';
		private const char ISLAND_MARKER = '#';
		private const char SHIP_MARKER = 'S';
		private const char ITEM_HOLDER_MARKER = 'H';

		private int width;
		private int height;
		private TileType[] map;

		[IgnoreDataMember] private string pattern;
		[MetaMember(1)] public string Pattern {
			get => pattern;
			set {
				pattern = value;
				string[] lines = value.Split(LINE_SEPARATOR);
				width = lines[0].Length;
				height = lines.Length;
				map = new TileType[width * height];

				for (int i = 0; i < width; i++) {
					for (int j = 0; j < height; j++) {
						map[j * width + i] = ParseType(lines[height - j - 1][i]);
					}
				}
			}
		}

		[IgnoreDataMember]
		public TileType this[int x, int y] {
			get {
				if (x < 0 || x >= width) {
					return TileType.Sea;
				}

				if (y < 0 || y >= height) {
					return TileType.Sea;
				}

				return map[y * width + x];
			}
		}

		public TileType TileAt(int index) => map[index];

		private TileType ParseType(char tileChar) {
			switch (tileChar) {
				case ISLAND_MARKER:      return TileType.Ground;
				case ITEM_HOLDER_MARKER: return TileType.ItemHolder;
				case SHIP_MARKER: return TileType.Ship;
				default:                 return TileType.Sea;
			}
		}
	}

	public enum TileType {
		Sea,
		Ground,
		ItemHolder,
		Ship
	}
}
