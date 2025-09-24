using System;

namespace SerousCommonLib.API.ModCall;

/// <summary>
/// An argument for a <see cref="BaseCallFunction"/> representing either a value of type <typeparamref name="T"/> or an optional argument (nothing).
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct NothingOr<T> {
	private readonly bool _isValue;
	private readonly T _value;

	/// <summary>
	/// <see langword="true"/> if the current instance is an undefined optional argument (nothing).
	/// </summary>
	public bool IsNothing => !_isValue;

	/// <summary>
	/// <see langword="true"/> if the current instance has a value.
	/// </summary>
	public bool IsValue => _isValue;

	/// <summary>
	/// The underlying value if <see cref="IsValue"/> is <see langword="true"/>, otherwise <c>default</c>.
	/// </summary>
	public T Value => IsValue ? _value : default;

	// Parameterless ctor = default, implicit

	/// <summary>
	/// Creates a new <see cref="NothingOr{T}"/> with an underlying value.
	/// </summary>
	/// <param name="value">The value</param>
	public NothingOr(T value) {
		_isValue = true;
		_value = value;
	}

	static NothingOr() {
		if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(NothingOr<>))
			throw new InvalidOperationException($"Cannot use {nameof(NothingOr<T>)}<{nameof(T)}> as a type parameter for itself.");
	}
}
