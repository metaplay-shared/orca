using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Orca.Common {
	/// <summary>
	/// Provides convenient extensions for options.
	/// </summary>
	public static class OptionExtensions {
		/// <summary>
		/// Wraps any value into an option.
		/// </summary>
		/// <param name="value">Any value; Can be null</param>
		/// <typeparam name="TValue">The value to be wrapped</typeparam>
		/// <returns>An option containing the value</returns>
		public static Option<TValue> ToOption<TValue>([NoEnumeration] this TValue value) => new Option<TValue>(value);

		/// <summary>
		/// Wraps any nullable value into an option.
		/// </summary>
		/// <param name="value">Any value; Can be null</param>
		/// <typeparam name="TValue">The value to be wrapped</typeparam>
		/// <returns>An option containing the value or none</returns>
		public static Option<TValue> ToOption<TValue>(this TValue? value) where TValue : struct =>
			value.HasValue ? new Option<TValue>(value.Value) : default;

		/// <summary>
		/// Flattens an option containing another option to just the inner option.
		/// </summary>
		/// <param name="nestedOptions">The nested options</param>
		/// <typeparam name="TValue">The value type of the inner option</typeparam>
		/// <returns>The inner option or a non-value option</returns>
		public static Option<TValue> Flatten<TValue>(this Option<Option<TValue>> nestedOptions) {
			return nestedOptions.GetOrElse(Option<TValue>.None);
		}

		/// <summary>
		/// Also known as Flat-Map. It is similar to map but requires the map function to return an option itself.
		/// The result is then flattened to avoid the build up of nested options.
		/// </summary>
		/// <param name="option">A option</param>
		/// <param name="map">Function that returns another option</param>
		/// <typeparam name="TValue">The value type of the option</typeparam>
		/// <typeparam name="TResult">The type of the resulting option value</typeparam>
		/// <returns>An option containing a value of the result type</returns>
		public static Option<TResult> Bind<TValue, TResult>(
			this Option<TValue> option,
			Func<TValue, Option<TResult>> map
		) {
			return option.Map(map).Flatten();
		}

		/// <summary>
		/// Also known as Flat-Map. It is similar to map but requires the map function to return an option itself.
		/// The result is then flattened to avoid the build up of nested options.
		/// Allows to specify a context parameter to avoid closure allocations.
		/// </summary>
		/// <param name="option">A option</param>
		/// <param name="context">A parameter that is passed to the map function</param>
		/// <param name="map">Function that returns another option</param>
		/// <typeparam name="TValue">The value type of the option</typeparam>
		/// <typeparam name="TResult">The type of the resulting option value</typeparam>
		/// <typeparam name="TContext">The type of the provided context parameter</typeparam>
		/// <returns>An option containing a value of the result type</returns>
		public static Option<TResult> Bind<TContext, TValue, TResult>(
			this Option<TValue> option,
			TContext context,
			Func<TContext, TValue, Option<TResult>> map
		) {
			return option.Map(context, map).Flatten();
		}

		/// <summary>
		///	Gets the values from options in a collection of options that have a value.
		/// </summary>
		/// <param name="collection">Collection of options to process</param>
		/// <typeparam name="TResult">Type of the entry in the collection</typeparam>
		/// <returns>A collection of values from the options</returns>
		public static IEnumerable<TResult> DiscardNones<TResult>(this IEnumerable<Option<TResult>> collection) {
			foreach (var option in collection)
			foreach (var value in option) {
				yield return value;
			}
		}

		/// <summary>
		/// Gets the <see cref="IEnumerable{TValue}"/> in an option or returns an empty
		/// <see cref="Enumerable"/> if the option has no value.
		/// </summary>
		/// <param name="option">Option of an <see cref="IEnumerable{TValue}"/></param>
		/// <typeparam name="TValue">The type of the elements of the <see cref="IEnumerable{TValue}"/></typeparam>
		/// <returns>The <see cref="IEnumerable{TValue}"/> in the option or an empty <see cref="Enumerable"/></returns>
		public static IEnumerable<TValue> GetOrEmpty<TValue>(this Option<IEnumerable<TValue>> option) =>
			option.GetOrElseLazy(() => Enumerable.Empty<TValue>());


		/// <summary>
		/// Ensures that the source collection is not empty.
		/// </summary>
		/// <param name="source">Collection to filter</param>
		/// <typeparam name="TSource">Type of the collection entries</typeparam>
		/// <returns>Returns source if the it is not empty; None if the source collection value is empty</returns>
		public static Option<IEnumerable<TSource>> FilterEmpty<TSource>(this Option<IEnumerable<TSource>> source) {
			return source.Bind(FilterEmpty);
		}

		/// <summary>
		/// Ensures that the source collection is not empty.
		/// </summary>
		/// <param name="source">Collection to filter</param>
		/// <typeparam name="TSource">Type of the collection entries</typeparam>
		/// <returns>Returns source if the it is not empty; None if the source collection value is empty</returns>
		public static Option<IEnumerable<TSource>> FilterEmpty<TSource>(this IEnumerable<TSource> source) {
			return source.Any() ? source.ToOption() : default;
		}
	}
}
