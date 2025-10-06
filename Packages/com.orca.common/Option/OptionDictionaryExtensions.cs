using System.Collections.Generic;

namespace Orca.Common {
	public static class OptionDictionaryExtensions {
		/// <summary>
		/// Gets the value associated with the specified key as an Option
		/// </summary>
		/// <param name="source">Source Dictionary</param>
		/// <param name="key">The key of the value to get</param>
		/// <typeparam name="TKey">The type of the key in the source Dictionary</typeparam>
		/// <typeparam name="TValue">The type of the value in the source Dictionary</typeparam>
		/// <returns>Option with the value if the Dictionary contains the value; otherwise Option.None</returns>
		public static Option<TValue> GetValueAsOption<TKey, TValue>(
			this IReadOnlyDictionary<TKey, TValue> source,
			TKey key
		) {
			return source.ContainsKey(key) ? source[key].ToOption() : Option<TValue>.None;
		}
	}
}
