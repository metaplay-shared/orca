using Metaplay.Core;

namespace Game.Logic {
	public static class SetUtils {
		public static T OneElement<T>(OrderedSet<T> s) {
			var e = s.GetEnumerator();
			e.MoveNext();
			T value = e.Current;
			e.Dispose();
			return value;
		}
	}
}
