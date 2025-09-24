using System;

namespace SerousCommonLib.API.ModCall;

/// <summary>
/// An argument for a <see cref="BaseCallFunction"/> representing a choice between two types.
/// </summary>
/// <typeparam name="TFirst">The first possible type.</typeparam>
/// <typeparam name="TSecond">The second possible type.</typeparam>
public readonly struct Either<TFirst, TSecond> {
	private readonly bool _isFirst;
	private readonly bool _isSecond;
	private readonly TFirst _first;
	private readonly TSecond _second;

	/// <summary>
	/// <see langword="true"/> if the underlying value is of type <typeparamref name="TFirst"/>
	/// </summary>
	public bool IsFirstOption => _isFirst;

	/// <summary>
	/// <see langword="true"/> if the underlying value is of type <typeparamref name="TSecond"/>
	/// </summary>
	public bool IsSecondOption => _isSecond;

	/// <summary>
	/// The underlying value if it is of type <typeparamref name="TFirst"/>, otherwise <c>default</c>.
	/// </summary>
	public TFirst FirstOption => IsFirstOption ? _first : default;

	/// <summary>
	/// The underlying value if it is of type <typeparamref name="TSecond"/>, otherwise <c>default</c>.
	/// </summary>
	public TSecond SecondOption => IsSecondOption ? _second : default;

	// Parameterless ctor = default, implicit

	/// <summary>
	/// Creates a new <see cref="Either{TFirst, TSecond}"/> with an underlying value of type <typeparamref name="TFirst"/>
	/// </summary>
	/// <param name="first">The value</param>
	public Either(TFirst first) {
		_isFirst = true;
		_isSecond = false;
		_first = first;
		_second = default;
	}

	/// <summary>
	/// Creates a new <see cref="Either{TFirst, TSecond}"/> with an underlying value of type <typeparamref name="TSecond"/>
	/// </summary>
	/// <param name="second">The value</param>
	public Either(TSecond second) {
		_isFirst = false;
		_isSecond = true;
		_first = default;
		_second = second;
	}

	static Either() {
		if (typeof(TFirst).IsGenericType && typeof(TFirst).GetGenericTypeDefinition() == typeof(Either<,>)
		|| typeof(TSecond).IsGenericType && typeof(TSecond).GetGenericTypeDefinition() == typeof(Either<,>))
			throw new InvalidOperationException($"Cannot use {nameof(Either<TFirst, TSecond>)}<{nameof(TFirst)}, {nameof(TSecond)}> as a type parameter for itself.");
	}
}
