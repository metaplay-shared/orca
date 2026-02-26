using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Orca.Common {
	public static class OptionLinqExtensions {
		/// <summary>
		///     Wrapper method for
		///     <see cref="Enumerable.ElementAtOrDefault{T}">ElementAtOrDefault&lt;T&gt;</see> converted into
		///     <see cref="Option{TValue}" />
		///     <inheritdoc cref="Enumerable.ElementAtOrDefault{T}" />
		/// </summary>
		/// <inheritdoc cref="Enumerable.ElementAtOrDefault{T}" />
		public static Option<TSource> ElementAtAsOption<TSource>(this IEnumerable<TSource> source, int index) {
			if (source == null) {
				Debug.LogWarning($"Tried to get {nameof(ElementAtAsOption)} from null source.");
				return default;
			}

			// Can't use the standard implementation for ElementAtOrDefault here
			// because for value types default is a valid option.
			try {
				return source.ElementAt(index);
			} catch (Exception) {
				return default;
			}
		}

		/// <summary>
		/// Gets the first element of the enumeration as an option or
		/// <see cref="Option{TValue}.None"/> if it is empty.
		/// <remarks>This will automatically convert the first element of the list to
		/// <see cref="Option{TValue}.None"/> if it is null.</remarks>
		/// </summary>
		/// <param name="source">An enumeration</param>
		/// <returns>The first element of the enumeration</returns>
		public static Option<TSource> FirstAsOption<TSource>(
			this IEnumerable<TSource> source
		) {
			if (source == null) {
				Debug.LogWarning($"Tried to get {nameof(FirstAsOption)} from null source.");
				return default;
			}

			using var enumerator = source.GetEnumerator();
			return enumerator.MoveNext() ? enumerator.Current.ToOption() : default;
		}

		/// <summary>
		///     Wrapper method for
		///     <see cref="Enumerable.FirstOrDefault{T}(IEnumerable{T}, Func{T, bool})" /> converted into
		///     <see cref="Option{TValue}" />
		///     <inheritdoc cref="Enumerable.FirstOrDefault{T}(IEnumerable{T}, Func{T, bool})" />
		/// </summary>
		/// <inheritdoc cref="Enumerable.FirstOrDefault{T}(IEnumerable{T}, Func{T, bool})" />
		public static Option<TSource> FirstAsOption<TSource>(
			this IEnumerable<TSource> source,
			Func<TSource, bool> predicate
		) {
			if (source == null) {
				Debug.LogWarning($"Tried to get {nameof(FirstAsOption)} from null source.");
				return default;
			}

			// Can't use the standard implementation for FirstOrDefault here
			// because for value types default is a valid option.
			foreach (var element in source) {
				if (predicate(element)) {
					return element;
				}
			}

			return default;
		}
		
		/// <summary>
		///     Wrapper method for
		///     <see cref="Enumerable.FirstOrDefault{T}(IEnumerable{T}, Func{T, bool})" /> converted into
		///     <see cref="Option{TValue}" />
		///     <inheritdoc cref="Enumerable.FirstOrDefault{T}(IEnumerable{T}, Func{T, bool})" />
		/// </summary>
		/// <inheritdoc cref="Enumerable.FirstOrDefault{T}(IEnumerable{T}, Func{T, bool})" />
		public static Option<TSource> FirstAsOption<TSource, TContext>(
			this IEnumerable<TSource> source,
			TContext context,
			Func<TContext, TSource, bool> predicate
		) {
			if (source == null) {
				Debug.LogWarning($"Tried to get {nameof(FirstAsOption)} from null source.");
				return default;
			}

			// Can't use the standard implementation for FirstOrDefault here
			// because for value types default is a valid option.
			foreach (var element in source) {
				if (predicate(context, element)) {
					return element;
				}
			}

			return default;
		}
	}
}
