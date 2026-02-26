using System;
using System.Runtime.Serialization;
using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class LockAreaModel {
		private const char LINE_SEPARATOR = '|';
		public const char NO_AREA = '.';

		private int width;
		private int height;
		[IgnoreDataMember] private char[] map;

		[MetaMember(1)] public MetaDictionary<char, AreaState> Areas { get; private set; }

		[IgnoreDataMember] private string pattern;
		[MetaMember(2)] public string Pattern {
			get => pattern;
			set {
				pattern = value.Trim('"');
				string[] lines = pattern.Split(LINE_SEPARATOR);
				width = lines[0].Length;
				height = lines.Length;
				map = new char[width * height];

				for (int i = 0; i < width; i++) {
					for (int j = 0; j < height; j++) {
						map[j * width + i] = lines[height - j - 1][i];
					}
				}
			}
		}

		public LockAreaModel() {}

		public LockAreaModel(string pattern) {
			Areas = new MetaDictionary<char, AreaState>();
			Pattern = pattern;
		}

		[IgnoreDataMember]
		public char this[int x, int y] {
			get {
				if (x < 0 || x >= width) {
					return NO_AREA;
				}

				if (y < 0 || y >= height) {
					return NO_AREA;
				}

				return map[y * width + x];
			}
		}

		public char TileAt(int index) => map[index];

		public int GetCloudCode(int x, int y) {
			char index = this[x, y];
			if (index == NO_AREA) {
				return -1;
			}

			int bit0 = IsSameCloud(x, y - 1, index) ? 0 : 1;
			int bit1 = IsSameCloud(x + 1, y, index) ? 0 : 1;
			int bit2 = IsSameCloud(x, y + 1, index) ? 0 : 1;
			int bit3 = IsSameCloud(x - 1, y, index) ? 0 : 1;

			int bit4 = !IsSameCloud(x - 1, y - 1, index) && bit3 == 0 && bit0 == 0 ? 1 : 0;
			int bit5 = !IsSameCloud(x + 1, y - 1, index) && bit0 == 0 && bit1 == 0 ? 1 : 0;
			int bit6 = !IsSameCloud(x + 1, y + 1, index) && bit1 == 0 && bit2 == 0 ? 1 : 0;
			int bit7 = !IsSameCloud(x - 1, y + 1, index) && bit2 == 0 && bit3 == 0 ? 1 : 0;

			return bit7 << 7 | bit6 << 6 | bit5 << 5 | bit4 << 4 | bit3 << 3 | bit2 << 2 | bit1 << 1 | bit0;
		}

		private bool IsSameCloud(int x, int y, char index) {
			return this[x, y] == index;
		}

		public bool IsFree(int x, int y) {
			char areaIndex = this[x, y];
			if (areaIndex == NO_AREA) {
				return true;
			}

			return Areas.GetValueOrDefault(areaIndex) == AreaState.Open;
		}

		public void LockArea(char area) {
			Areas[area] = AreaState.Locked;
		}

		public void UnlockArea(char area) {
			Areas[area] = AreaState.Opening;
		}

		public void OpenArea(char area) {
			Areas[area] = AreaState.Open;
		}

		public AreaState AreaLockState(char areaIndex) => Areas.GetValueOrDefault(areaIndex);

		// Checks whether the dependencies (if there are any) of a lock area are already open.
		public bool DependenciesOpen(char areaIndex, IslandTypeId island, SharedGameConfig gameConfig) {
			LockAreaId lockAreaId = new LockAreaId(island, areaIndex.ToString());
			if (!gameConfig.LockAreas.TryGetValue(lockAreaId, out LockAreaInfo lockAreaInfo)) {
				return false;
			}
			string dependency = lockAreaInfo?.Dependency;
			if (string.IsNullOrEmpty(dependency)) {
				return true;
			}

			return Areas.GetValueOrDefault(dependency[0]) == AreaState.Open;
		}
	}

	[MetaSerializable]
	public enum AreaState {
		Locked,
		Opening,
		Open
	}
}
