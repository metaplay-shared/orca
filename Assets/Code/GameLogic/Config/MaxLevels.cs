using System.Collections;
using Metaplay.Core;

namespace Game.Logic {
	public class MaxLevels<T> where T : StringId<T>, new() {
		public MetaDictionary<T, int> Levels = new();

		public MaxLevels(IEnumerable keys) {
			foreach (LevelId<T> key in keys) {
				int oldLevel = Levels.GetValueOrDefault(key.Type);
				if (key.Level > oldLevel) {
					Levels[key.Type] = key.Level;
				}
			}
		}

		public int GetMaxLevel(T type) {
			return Levels.GetValueOrDefault(type);
		}
	}
}
