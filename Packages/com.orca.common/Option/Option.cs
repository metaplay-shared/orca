using System;

namespace Orca.Common {
	/// <summary>
	/// Wraps a type to perform operations on it based on if it carries a value.
	/// Use it to avoid null checks in code. Create an option from any object that might be null.
	/// It can also used on structs to communicate a maybe value.
	/// </summary>
	/// <typeparam name="TValue">The wrapped type; Can be anything</typeparam>
	public readonly struct Option<TValue> {
		public readonly bool HasValue;
		private readonly TValue value;

		/// <summary>
		/// Constructs an option from a given value. The value may be null.
		/// </summary>
		/// <param name="value">Any object or struct;
		/// If a provided object is null, the option won't have a value</param>
		public Option(TValue value) {
			this.value = value;
			this.HasValue = default(TValue) != null || this.value != null;
		}

		/// <summary>
		/// Represents a non-value option of that type.
		/// </summary>
		public static Option<TValue> None => new Option<TValue>();

		/// <summary>
		/// Implicit conversion of any value to an option.
		/// Reduces amount of writing when introducing option to a code base.
		/// </summary>
		/// <param name="value">Any object or struct;
		/// If a provided object is null, the option won't have a value</param>
		/// <returns>The provided value wrapped into an option</returns>
		public static implicit operator Option<TValue>(TValue value) => new Option<TValue>(value);

		/// <summary>
		/// Performs the provided actions whether the option has a value or not.
		/// </summary>
		/// <param name="some">Action performed if the option has a value;
		/// The value is used as a parameter for the action</param>
		/// <param name="none">Action performed if the option has no value</param>
		public void Match(Action<TValue> some, Action none) {
			if (this.HasValue) {
				some(value);
			} else {
				none();
			}
		}

		/// <summary>
		/// Performs the provided actions whether the option has a value or not.
		/// Allows to specify a context parameter to avoid closure allocations. 
		/// </summary>
		/// <param name="context">A parameter that is passed to the actions</param>
		/// <param name="some">Action performed if the option has a value;
		/// The value is used as a parameter for the action</param>
		/// <param name="none">Action performed if the option has no value</param>
		public void Match<TContext>(TContext context, Action<TContext, TValue> some, Action<TContext> none) {
			if (this.HasValue) {
				some(context, value);
			} else {
				none(context);
			}
		}

		/// <summary>
		/// Performs the provided functions whether the option has a value or not.
		/// </summary>
		/// <param name="some">Function performed if the option has a value.
		/// The value is used as a parameter for the function</param>
		/// <param name="none">Function performed if the option has no value</param>
		/// <returns>The result of the executed function</returns>
		public TResult Match<TResult>(Func<TValue, TResult> some, Func<TResult> none) {
			return this.HasValue ? some(this.value) : none();
		}

		/// <summary>
		/// Performs the provided functions whether the option has a value or not.
		/// Allows to specify a context parameter to avoid closure allocations. 
		/// </summary>
		/// <param name="context">A parameter that is passed to the function.</param>
		/// <param name="some">Function performed if the option has a value;
		/// The value is used as a parameter for the function</param>
		/// <param name="none">Function performed if the option has no value</param>
		/// <returns>The result of the executed function</returns>
		public TResult Match<TContext, TResult>(
			TContext context,
			Func<TContext, TValue, TResult> some,
			Func<TContext, TResult> none
		) {
			return this.HasValue ? some(context, this.value) : none(context);
		}

		/// <summary>
		/// Performs the provided action when the option has a value.
		/// </summary>
		/// <param name="action">Action performed if the option has a value;
		/// The value is used as a parameter for the action</param>
		public void MatchSome(Action<TValue> action) {
			if (this.HasValue) {
				action(this.value);
			}
		}

		/// <summary>
		/// Performs the provided action when the option has a value.
		/// Allows to specify a context parameter to avoid closure allocations. 
		/// </summary>
		/// <param name="context">A parameter that is passed to the function</param>
		/// <param name="action">Action performed if the option has a value;
		/// The value is used as a parameter for the action</param>
		public void MatchSome<TContext>(TContext context, Action<TContext, TValue> action) {
			if (this.HasValue) {
				action(context, this.value);
			}
		}

		/// <summary>
		/// Performs the provided action when the option has no value.
		/// </summary>
		/// <param name="action">Action performed if the option has no value</param>
		public void MatchNone(Action action) {
			if (!this.HasValue) {
				action();
			}
		}

		/// <summary>
		/// Performs the provided action when the option has no value.
		/// Allows to specify a context parameter to avoid closure allocations. 
		/// </summary>
		/// <param name="context">A parameter that is passed to the action</param>
		/// <param name="action">Action performed if the option has no value</param>
		public void MatchNone<TContext>(TContext context, Action<TContext> action) {
			if (!this.HasValue) {
				action(context);
			}
		}

		/// <summary>
		/// Maps the value in the option to a new value and returns a new option with that new value.
		/// </summary>
		/// <param name="map">Function that converts the original value</param>
		/// <typeparam name="TResult">Type of the converted value</typeparam>
		/// <returns>An option containing the mapping result</returns>
		public Option<TResult> Map<TResult>(Func<TValue, TResult> map) =>
			this.HasValue ? map(value) : Option<TResult>.None;

		/// <summary>
		/// Maps the value in the option to a new value and returns a new option with that new value.
		/// Allows to specify a context parameter to avoid closure allocations. 
		/// </summary>
		/// <param name="context">A parameter that is passed to the map function</param>
		/// <param name="map">Function that converts the original value</param>
		/// <typeparam name="TResult">Type of the converted value</typeparam>
		/// <returns>An option containing the mapping result</returns>
		public Option<TResult> Map<TResult, TContext>(TContext context, Func<TContext, TValue, TResult> map) =>
			this.HasValue ? map(context, value) : Option<TResult>.None;

		/// <summary>
		/// Checks if the value in the option matches a given predicate.
		/// </summary>
		/// <param name="predicate">A predicate function that takes the options value as a parameter</param>
		/// <returns>The same option or a non-value option whether the predicate matches</returns>
		public Option<TValue> Filter(Func<TValue, bool> predicate) =>
			this.HasValue && predicate(this.value) ? this : None;

		/// <summary>
		/// Checks if the value in the option matches a given predicate.
		/// Allows to specify a context parameter to avoid closure allocations. 
		/// </summary>
		/// <param name="context">A parameter that is passed to the predicate function</param>
		/// <param name="predicate">A predicate function that takes the options value as a parameter</param>
		/// <returns>The same option or a non-value option whether the predicate matches</returns>
		public Option<TValue> Filter<TContext>(TContext context, Func<TContext, TValue, bool> predicate) =>
			this.HasValue && predicate(context, this.value) ? this : None;

		/// <summary>
		/// Checks whether the value in the option matches a given predicate.
		/// </summary>
		/// <param name="predicate">A predicate function that takes the options value</param>
		/// <returns>True if the containing value matches the given predicate</returns>
		public bool Exists(Func<TValue, bool> predicate) => this.HasValue && predicate(this.value);

		/// <summary>
		/// Checks whether the value in the option matches a given predicate.
		/// Allows to specify a context parameter to avoid closure allocations. 
		/// </summary>
		/// <param name="context">A parameter that is passed to the predicate function</param>
		/// <param name="predicate">A predicate function that takes the options value</param>
		/// <returns>True if the containing value matches the given predicate</returns>
		public bool Exists<TContext>(TContext context, Func<TContext, TValue, bool> predicate) =>
			this.HasValue && predicate(context, this.value);

		/// <summary>
		/// Retrieve the value of the option or a provided value if the option has no value.
		/// </summary>
		/// <param name="other">The value that is returned if the option has no value</param>
		/// <returns>Either the options value or the provided value</returns>
		public TValue GetOrElse(TValue other) => this.HasValue ? this.value : other;

		/// <summary>
		/// Retrieve the value of the option or a provided value if the option has no value.
		/// "Lazy" version to execute a getter function to determine the return value if the option has no value.
		/// </summary>
		/// <param name="getOther">Function to determine a value that is returned if the option has no value.</param>
		/// <returns>Either the options value or the calculated value of the get function</returns>
		public TValue GetOrElseLazy(Func<TValue> getOther) => this.HasValue ? this.value : getOther();

		/// <summary>
		/// Retrieve the value of the option or a provided value if the option has no value.
		/// "Lazy" version to execute a getter function to determine the return value if the option has no value.
		/// Allows to specify a context parameter to avoid closure allocations. 
		/// </summary>
		/// <param name="context">A parameter that is passed to the getter function</param>
		/// <param name="getOther">Function to determine a value that is returned if the option has no value.</param>
		/// <returns>Either the options value or the calculated value of the get function</returns>
		public TValue GetOrElseLazy<TContext>(TContext context, Func<TContext, TValue> getOther) =>
			this.HasValue ? this.value : getOther(context);

		/// <summary>
		/// Used to allow accessing the options value in a for-each syntax.
		/// </summary>
		/// <returns>Enumerator that provides the options value.</returns>
		public OptionEnumerator GetEnumerator() =>
			this.HasValue ? new OptionEnumerator(this.value) : new OptionEnumerator();

		public struct OptionEnumerator {
			private bool hasValue;

			public OptionEnumerator(TValue value) {
				Current = value;
				this.hasValue = true;
			}

			public bool MoveNext() {
				if (!this.hasValue) {
					return false;
				}

				this.hasValue = false;
				return true;
			}

			public TValue Current { get; }
		}
	}
}
