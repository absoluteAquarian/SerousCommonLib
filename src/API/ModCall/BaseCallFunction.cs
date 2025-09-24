using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Terraria.ModLoader;

namespace SerousCommonLib.API.ModCall;

#nullable enable

/// <summary>
/// The base class for <see cref="Mod.Call(object[])"/> handlers
/// </summary>
public abstract class BaseCallFunction : ModType {
	private class StaticLoadable : ILoadable {
		void ILoadable.Load(Mod mod) { }

		void ILoadable.Unload() {
			_callFunctionLookup.Clear();
		}
	}

	private static readonly string[] _indexToName = [
		"1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th",
		"11th", "12th", "13th", "14th", "15th", "16th", "17th", "18th", "19th", "20th",
	];

	/// <summary>
	/// The first argument to be passed to <see cref="Mod.Call(object[])"/> to invoke this call function.<br/>
	/// Defaults <see cref="ModType.Name"/> with proper word separation. (e.g. "MyFunction" -> "My Function")
	/// </summary>
	public virtual string Function => PrettyPrintName();

	private static readonly Dictionary<string, Dictionary<string, BaseCallFunction>> _callFunctionLookup = [];

	/// <summary>
	/// Attempts to find a <see cref="BaseCallFunction"/> in the given mod with the given <see cref="Function"/><br/>
	/// Throws a <see cref="KeyNotFoundException"/> if one could not be found.
	/// </summary>
	/// <param name="mod">The mod which contains the <see cref="BaseCallFunction"/></param>
	/// <param name="function">The <see cref="Function"/> to look for</param>
	/// <returns>The found <see cref="BaseCallFunction"/></returns>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="KeyNotFoundException"/>
	public static BaseCallFunction Find(Mod mod, string function) {
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(function);
		
		if (!TryFind(mod, function, out var callFunction))
			throw new KeyNotFoundException($"Call function \"{function}\" not found in mod \"{mod.Name}\"");

		return callFunction;
	}

	/// <summary>
	/// Attempts to find a <see cref="BaseCallFunction"/> in the given mod with the given <see cref="Function"/>
	/// </summary>
	/// <param name="mod">The mod which contains the <see cref="BaseCallFunction"/></param>
	/// <param name="function">The <see cref="Function"/> to look for</param>
	/// <param name="callFunction">The found <see cref="BaseCallFunction"/>, or <see langword="null"/> if one could not be found</param>
	/// <returns><see langword="true"/> if a <see cref="BaseCallFunction"/> was found; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"/>
	public static bool TryFind(Mod mod, string function, [NotNullWhen(true)] out BaseCallFunction? callFunction) {
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(function);

		if (_callFunctionLookup.TryGetValue(mod.Name, out var functionLookup) && functionLookup.TryGetValue(function, out callFunction))
			return true;

		foreach (var func in mod.GetContent<BaseCallFunction>()) {
			if (func.Function == function) {
				if (!_callFunctionLookup.TryGetValue(mod.Name, out functionLookup))
					_callFunctionLookup[mod.Name] = functionLookup = [];

				functionLookup[function] = callFunction = func;
				return true;
			}
		}

		callFunction = null;
		return false;
	}

	/// <summary>
	/// The function which handles the arguments and return value for the call function
	/// </summary>
	public abstract object Call(ReadOnlySpan<object> args);

	/// <inheritdoc/>
	protected sealed override void Register() => ModTypeLookup<BaseCallFunction>.Register(this);

	/// <inheritdoc/>
	protected sealed override void InitTemplateInstance() { }

	/// <inheritdoc/>
	protected sealed override void ValidateType() { }

	/// <summary>
	/// Gets the argument at <paramref name="index"/> from <paramref name="args"/> as a <typeparamref name="T"/><br/>
	/// Throws a <see cref="ArgumentException"/> if the argument is not of type <typeparamref name="T"/> or is <see langword="null"/>.
	/// </summary>
	/// <typeparam name="T">The expected type of the argument</typeparam>
	/// <param name="args">The argument list</param>
	/// <param name="index">The index of the argument to get</param>
	/// <returns>The argument at <paramref name="index"/> as a <typeparamref name="T"/></returns>
	/// <exception cref="ArgumentException"/>
	[StackTraceHidden]
	protected T GetOrThrowIfNot<T>(ReadOnlySpan<object> args, int index) {
		if (args[index] is T value)
			return value;

		string argName = _indexToName.Length > index ? $"the {_indexToName[index]} argument" : $"argument {index + 1}";
		string expectedType = typeof(T).GetSimplifiedGenericTypeName();
		string actualType = args[index]?.GetType().GetSimplifiedGenericTypeName() ?? "null";
		throw new ArgumentException($"Call \"{Function}\" requires {argName} to be of type {expectedType}, but got {actualType} instead");
	}

	/// <summary>
	/// Gets the argument at <paramref name="index"/> from <paramref name="args"/> as a <typeparamref name="T"/>, or <see langword="null"/> if the argument is <see langword="null"/>.<br/>
	/// Throws a <see cref="ArgumentException"/> if the argument is not of type <typeparamref name="T"/> nor <see langword="null"/>.
	/// </summary>
	/// <typeparam name="T">The expected type of the argument</typeparam>
	/// <param name="args">The argument list</param>
	/// <param name="index">The index of the argument to get</param>
	/// <returns>The argument at <paramref name="index"/> as a <typeparamref name="T"/>, or <see langword="null"/> if the argument is <see langword="null"/></returns>
	/// <exception cref="ArgumentException"/>
	[StackTraceHidden]
	protected T GetOrThrowIfNotNull<T>(ReadOnlySpan<object> args, int index) where T : class {
		if (args[index] is T value)
			return value;
		if (args[index] is null)
			return null!;

		string argName = _indexToName.Length > index ? $"the {_indexToName[index]} argument" : $"argument {index + 1}";
		string expectedType = typeof(T).GetSimplifiedGenericTypeName();
		string actualType = args[index].GetType().GetSimplifiedGenericTypeName();
		throw new ArgumentException($"Call \"{Function}\" requires {argName} to be of type {expectedType}, but got {actualType} instead");
	}

	/// <summary>
	/// Gets the (optional) argument at <paramref name="index"/> from <paramref name="args"/> as a <see cref="NothingOr{T}"/><br/>
	/// Throws a <see cref="ArgumentException"/> if the argument is not of type <typeparamref name="T"/> nor <see langword="null"/>.
	/// </summary>
	/// <typeparam name="T">The expected type of the argument</typeparam>
	/// <param name="args">The argument list</param>
	/// <param name="index">The index of the argument to get.  Can exceed the length of <paramref name="args"/>.</param>
	/// <returns>The argument at <paramref name="index"/> as a <see cref="NothingOr{T}"/>, or an empty <see cref="NothingOr{T}"/> if the argument is <see langword="null"/> or <paramref name="index"/> exceeds the length of <paramref name="args"/></returns>
	/// <exception cref="ArgumentException"/>
	[StackTraceHidden]
	protected NothingOr<T> GetOrThrowIfNotNothing<T>(ReadOnlySpan<object> args, int index) {
		if (index >= args.Length || args[index] is null)
			return default;

		if (args[index] is T value)
			return new NothingOr<T>(value);

		string argName = _indexToName.Length > index ? $"the {_indexToName[index]} argument" : $"argument {index + 1}";
		string expectedType = typeof(T).GetSimplifiedGenericTypeName();
		string actualType = args[index].GetType().GetSimplifiedGenericTypeName();
		throw new ArgumentException($"Call \"{Function}\" requires {argName} to be of type {expectedType}, but got {actualType} instead");
	}

	/// <summary>
	/// Gets the argument at <paramref name="index"/> from <paramref name="args"/> as a <see cref="Either{TFirst, TSecond}"/><br/>
	/// Throws a <see cref="ArgumentException"/> if the argument is not of type <typeparamref name="TFirst"/> nor <typeparamref name="TSecond"/>
	/// </summary>
	/// <typeparam name="TFirst">The first possible type of the argument</typeparam>
	/// <typeparam name="TSecond">The second possible type of the argument</typeparam>
	/// <param name="args">The argument list</param>
	/// <param name="index">The index of the argument to get</param>
	/// <returns>The argument at <paramref name="index"/> as a <see cref="Either{TFirst, TSecond}"/></returns>
	/// <exception cref="ArgumentException"/>
	[StackTraceHidden]
	protected Either<TFirst, TSecond> GetOrThrowIfNotEither<TFirst, TSecond>(ReadOnlySpan<object> args, int index) {
		var arg = args[index];

		if (arg is TFirst first)
			return new Either<TFirst, TSecond>(first);
		if (arg is TSecond second)
			return new Either<TFirst, TSecond>(second);

		string argName = _indexToName.Length > index ? $"the {_indexToName[index]} argument" : $"argument {index + 1}";
		string expectedType = $"{typeof(TFirst).GetSimplifiedGenericTypeName()} or {typeof(TSecond).GetSimplifiedGenericTypeName()}";
		string actualType = arg?.GetType().GetSimplifiedGenericTypeName() ?? "null";
		throw new ArgumentException($"Call \"{Function}\" requires {argName} to be of type {expectedType}, but got {actualType} instead");
	}

	/// <summary>
	/// Gets the argument at <paramref name="index"/> from <paramref name="args"/> as a <see cref="OneOfMany{T1, T2, T3}"/><br/>
	/// Throws a <see cref="ArgumentException"/> if the argument is not of type <typeparamref name="T1"/>, <typeparamref name="T2"/> nor <typeparamref name="T3"/>
	/// </summary>
	/// <typeparam name="T1">The first possible type of the argument</typeparam>
	/// <typeparam name="T2">The second possible type of the argument</typeparam>
	/// <typeparam name="T3">The third possible type of the argument</typeparam>
	/// <param name="args">The argument list</param>
	/// <param name="index">The index of the argument to get</param>
	/// <returns>The argument at <paramref name="index"/> as a <see cref="OneOfMany{T1, T2, T3}"/></returns>
	/// <exception cref="ArgumentException"/>
	[StackTraceHidden]
	protected OneOfMany<T1, T2, T3> GetOrThrowIfNotAny<T1, T2, T3>(ReadOnlySpan<object> args, int index) {
		var arg = args[index];

		if (arg is T1 value1)
			return new OneOfMany<T1, T2, T3>(value1);
		if (arg is T2 value2)
			return new OneOfMany<T1, T2, T3>(value2);
		if (arg is T3 value3)
			return new OneOfMany<T1, T2, T3>(value3);

		string argName = _indexToName.Length > index ? $"the {_indexToName[index]} argument" : $"argument {index + 1}";
		string expectedType = $"{typeof(T1).GetSimplifiedGenericTypeName()}, {typeof(T2).GetSimplifiedGenericTypeName()} or {typeof(T3).GetSimplifiedGenericTypeName()}";
		string actualType = arg?.GetType().GetSimplifiedGenericTypeName() ?? "null";
		throw new ArgumentException($"Call \"{Function}\" requires {argName} to be of type {expectedType}, but got {actualType} instead");
	}

	/// <summary>
	/// Throws an <see cref="ArgumentException"/> if <paramref name="args"/> does not contain at least <paramref name="minArgs"/> elements.
	/// </summary>
	/// <param name="args">The argument list</param>
	/// <param name="minArgs">The minimum number of arguments required</param>
	/// <exception cref="ArgumentException"/>
	[StackTraceHidden]
	protected void ThrowIfNotEnoughArgs(ReadOnlySpan<object> args, int minArgs) {
		if (args.Length < minArgs)
			throw new ArgumentException($"Call \"{Function}\" requires at least {minArgs} arguments, but got {args.Length} instead");
	}

	/// <summary>
	/// Throws an <see cref="ArgumentException"/> with the given <paramref name="message"/>, indicating that the argument at <paramref name="argumentIndex"/> is invalid.
	/// </summary>
	/// <param name="message">The reason why the argument is invalid</param>
	/// <param name="argumentIndex">The index of the invalid argument</param>
	/// <exception cref="ArgumentException"/>
	[StackTraceHidden]
	[DoesNotReturn]
	protected void ThrowWithMessage(string message, int argumentIndex) {
		string argName = _indexToName.Length > argumentIndex ? $"the {_indexToName[argumentIndex]} argument" : $"argument {argumentIndex + 1}";
		throw new ArgumentException($"Call \"{Function}\" could not be performed due to {argName} being invalid.\nReason: {message}");
	}
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunction"/> which does not take any arguments.
/// </summary>
public abstract class BaseCallFunctionNoArgs : BaseCallFunction {
	/// <inheritdoc/>
	public sealed override object Call(ReadOnlySpan<object> args) => Handle();

	/// <inheritdoc cref="Call"/>
	protected abstract object Handle();
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunction"/> which does not return a value.
/// </summary>
public abstract class BaseCallFunctionNoReturn : BaseCallFunction {
	/// <inheritdoc/>
	public sealed override object Call(ReadOnlySpan<object> args) {
		Handle(args);
		return null!;
	}

	/// <inheritdoc cref="Call"/>
	protected abstract void Handle(ReadOnlySpan<object> args);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunctionNoReturn"/> which takes one argument and does not return a value.
/// </summary>
/// <typeparam name="T">The type of the argument</typeparam>
public abstract class BaseCallFunctionNoReturn<T> : BaseCallFunctionNoReturn {
	/// <inheritdoc/>
	protected sealed override void Handle(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 1);
		Handle(
			GetOrThrowIfNot<T>(args, 0)
		);
	}

	/// <inheritdoc cref="Handle(ReadOnlySpan{object})"/>
	protected abstract void Handle(T arg);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunctionNoReturn"/> which takes two arguments and does not return a value.
/// </summary>
/// <typeparam name="T1">The type of the first argument</typeparam>
/// <typeparam name="T2">The type of the second argument</typeparam>
public abstract class BaseCallFunctionNoReturn<T1, T2> : BaseCallFunctionNoReturn {
	/// <inheritdoc/>
	protected sealed override void Handle(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 2);
		Handle(
			GetOrThrowIfNot<T1>(args, 0),
			GetOrThrowIfNot<T2>(args, 1)
		);
	}

	/// <inheritdoc cref="Handle(ReadOnlySpan{object})"/>
	protected abstract void Handle(T1 arg1, T2 arg2);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunctionNoReturn"/> which takes three arguments and does not return a value.
/// </summary>
/// <typeparam name="T1">The type of the first argument</typeparam>
/// <typeparam name="T2">The type of the second argument</typeparam>
/// <typeparam name="T3">The type of the third argument</typeparam>
public abstract class BaseCallFunctionNoReturn<T1, T2, T3> : BaseCallFunctionNoReturn {
	/// <inheritdoc/>
	protected sealed override void Handle(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 3);
		Handle(
			GetOrThrowIfNot<T1>(args, 0),
			GetOrThrowIfNot<T2>(args, 1),
			GetOrThrowIfNot<T3>(args, 2)
		);
	}

	/// <inheritdoc cref="Handle(ReadOnlySpan{object})"/>
	protected abstract void Handle(T1 arg1, T2 arg2, T3 arg3);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunctionNoReturn"/> which takes four arguments and does not return a value.
/// </summary>
/// <typeparam name="T1">The type of the first argument</typeparam>
/// <typeparam name="T2">The type of the second argument</typeparam>
/// <typeparam name="T3">The type of the third argument</typeparam>
/// <typeparam name="T4">The type of the fourth argument</typeparam>
public abstract class BaseCallFunctionNoReturn<T1, T2, T3, T4> : BaseCallFunctionNoReturn {
	/// <inheritdoc/>
	protected sealed override void Handle(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 4);
		Handle(
			GetOrThrowIfNot<T1>(args, 0),
			GetOrThrowIfNot<T2>(args, 1),
			GetOrThrowIfNot<T3>(args, 2),
			GetOrThrowIfNot<T4>(args, 3)
		);
	}

	/// <inheritdoc cref="Handle(ReadOnlySpan{object})"/>
	protected abstract void Handle(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunctionNoReturn"/> which takes five arguments and does not return a value.
/// </summary>
/// <typeparam name="T1">The type of the first argument</typeparam>
/// <typeparam name="T2">The type of the second argument</typeparam>
/// <typeparam name="T3">The type of the third argument</typeparam>
/// <typeparam name="T4">The type of the fourth argument</typeparam>
/// <typeparam name="T5">The type of the fifth argument</typeparam>
public abstract class BaseCallFunctionNoReturn<T1, T2, T3, T4, T5> : BaseCallFunctionNoReturn {
	/// <inheritdoc/>
	protected sealed override void Handle(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 5);
		Handle(
			GetOrThrowIfNot<T1>(args, 0),
			GetOrThrowIfNot<T2>(args, 1),
			GetOrThrowIfNot<T3>(args, 2),
			GetOrThrowIfNot<T4>(args, 3),
			GetOrThrowIfNot<T5>(args, 4)
		);
	}

	/// <inheritdoc cref="Handle(ReadOnlySpan{object})"/>
	protected abstract void Handle(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunction"/> which takes one argument and returns a value.
/// </summary>
/// <typeparam name="TArg">The type of the argument</typeparam>
/// <typeparam name="TReturn">The type of the return value</typeparam>
public abstract class BaseCallFunction<TArg, TReturn> : BaseCallFunction {
	/// <inheritdoc/>
	public sealed override object Call(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 1);
		return Handle(
			GetOrThrowIfNot<TArg>(args, 0)
		)!;
	}

	/// <inheritdoc cref="Call"/>
	protected abstract TReturn Handle(TArg arg);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunction"/> which takes two arguments and returns a value.
/// </summary>
/// <typeparam name="TArg1">The type of the first argument</typeparam>
/// <typeparam name="TArg2">The type of the second argument</typeparam>
/// <typeparam name="TReturn">The type of the return value</typeparam>
public abstract class BaseCallFunction<TArg1, TArg2, TReturn> : BaseCallFunction {
	/// <inheritdoc/>
	public sealed override object Call(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 2);
		return Handle(
			GetOrThrowIfNot<TArg1>(args, 0),
			GetOrThrowIfNot<TArg2>(args, 1)
		)!;
	}

	/// <inheritdoc cref="Call"/>
	protected abstract TReturn Handle(TArg1 arg1, TArg2 arg2);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunction"/> which takes three arguments and returns a value.
/// </summary>
/// <typeparam name="TArg1">The type of the first argument</typeparam>
/// <typeparam name="TArg2">The type of the second argument</typeparam>
/// <typeparam name="TArg3">The type of the third argument</typeparam>
/// <typeparam name="TReturn">The type of the return value</typeparam>
public abstract class BaseCallFunction<TArg1, TArg2, TArg3, TReturn> : BaseCallFunction {
	/// <inheritdoc/>
	public sealed override object Call(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 3);
		return Handle(
			GetOrThrowIfNot<TArg1>(args, 0),
			GetOrThrowIfNot<TArg2>(args, 1),
			GetOrThrowIfNot<TArg3>(args, 2)
		)!;
	}

	/// <inheritdoc cref="Call"/>
	protected abstract TReturn Handle(TArg1 arg1, TArg2 arg2, TArg3 arg3);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunction"/> which takes four arguments and returns a value.
/// </summary>
/// <typeparam name="TArg1">The type of the first argument</typeparam>
/// <typeparam name="TArg2">The type of the second argument</typeparam>
/// <typeparam name="TArg3">The type of the third argument</typeparam>
/// <typeparam name="TArg4">The type of the fourth argument</typeparam>
/// <typeparam name="TReturn">The type of the return value</typeparam>
public abstract class BaseCallFunction<TArg1, TArg2, TArg3, TArg4, TReturn> : BaseCallFunction {
	/// <inheritdoc/>
	public sealed override object Call(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 4);
		return Handle(
			GetOrThrowIfNot<TArg1>(args, 0),
			GetOrThrowIfNot<TArg2>(args, 1),
			GetOrThrowIfNot<TArg3>(args, 2),
			GetOrThrowIfNot<TArg4>(args, 3)
		)!;
	}

	/// <inheritdoc cref="Call"/>
	protected abstract TReturn Handle(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4);
}

/// <summary>
/// The base implementation of a <see cref="BaseCallFunction"/> which takes five arguments and returns a value.
/// </summary>
/// <typeparam name="TArg1">The type of the first argument</typeparam>
/// <typeparam name="TArg2">The type of the second argument</typeparam>
/// <typeparam name="TArg3">The type of the third argument</typeparam>
/// <typeparam name="TArg4">The type of the fourth argument</typeparam>
/// <typeparam name="TArg5">The type of the fifth argument</typeparam>
/// <typeparam name="TReturn">The type of the return value</typeparam>
public abstract class BaseCallFunction<TArg1, TArg2, TArg3, TArg4, TArg5, TReturn> : BaseCallFunction {
	/// <inheritdoc/>
	public sealed override object Call(ReadOnlySpan<object> args) {
		ThrowIfNotEnoughArgs(args, 5);
		return Handle(
			GetOrThrowIfNot<TArg1>(args, 0),
			GetOrThrowIfNot<TArg2>(args, 1),
			GetOrThrowIfNot<TArg3>(args, 2),
			GetOrThrowIfNot<TArg4>(args, 3),
			GetOrThrowIfNot<TArg5>(args, 4)
		)!;
	}

	/// <inheritdoc cref="Call"/>
	protected abstract TReturn Handle(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5);
}
