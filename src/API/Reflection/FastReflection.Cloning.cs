using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using System;
using System.Linq;

#nullable enable
namespace SerousCommonLib.API {
	partial class FastReflection {
		private static readonly Dictionary<Type, Func<object?, object?>> cloneFuncs = new();
		private static readonly Dictionary<Type, FieldInfo[]> cachedFieldInfoArrays = new();

		[ThreadStatic, Obsolete($"Use {nameof(DetectedObjectInstances)} instead")]
		private static HashSet<object> detectedObjectInstances;
		[ThreadStatic]
		private static int nestingLevel;

		private static HashSet<object> DetectedObjectInstances => detectedObjectInstances ??= new(ReferenceEqualityComparer.Instance);

		private static readonly MethodInfo FastReflection_DeepClone = typeof(FastReflection).GetMethod(nameof(DeepClone), BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(Type), typeof(object) })!;

		/// <summary>
		/// Performs a recursive deep cloning algorithm to clone <paramref name="instance"/>
		/// </summary>
		/// <param name="type">The type of the instance to clone</param>
		/// <param name="instance">The instance to clone</param>
		/// <returns>The cloned instance</returns>
		/// <remarks>
		/// <b>NOTE:</b>  This algorithm has a few caveats that should be kept in mind:<br/>
		/// <list type="number">
		/// <item>
		/// <see langword="null"/> values in members are copied directly.
		/// </item>
		/// <item>
		/// Any <see langword="unmanaged"/> members (meaning, types whose members do not contain any references) are copied directly.
		/// </item>
		/// <item>
		/// <see cref="Array"/> members will have their lengths preserved and each array element is deep cloned individually.
		/// </item>
		/// <item>
		/// <see cref="string"/> members are copied directly.
		/// </item>
		/// <item>
		/// Pointer and <see langword="ref struct"/> members are set to <see langword="default"/>.
		/// </item>
		/// <item>
		/// Members which cause recursion (e.g. obj1.field → obj2, obj2.field → obj1) may or may not be copied directly.<br/>
		/// Such members should be checked manually after deep cloning.
		/// </item>
		/// </list>
		/// </remarks>
		/// <exception cref="ArgumentException"/>
		[return: NotNullIfNotNull(nameof(instance))]
		public static object? DeepClone(this Type type, object? instance) {
			if (type.IsAbstract || (type.IsAbstract && type.IsSealed) || type.IsInterface || type.IsGenericTypeDefinition)
				throw new ArgumentException($"Type \"{type.GetSimplifiedGenericTypeName()}\" cannot be used with this method.");

			if (instance is null)
				return null;

			if (instance.GetType().IsClass)
				DetectedObjectInstances.Add(instance);

			Func<object?, object?> func = BuildDeepCloneDelegate(type);
			
			nestingLevel++;
			object obj = func(instance)!;
			nestingLevel--;

			// Only clear the dictionary if this DeepClone method wasn't called within another DeepClone method
			if (nestingLevel == 0)
				DetectedObjectInstances.Clear();

			return obj;
		}

		/// <inheritdoc cref="DeepClone(Type, object?)"/>
		[return: NotNullIfNotNull("instance")]
		public static T? DeepClone<T>(T? instance) => (T?)typeof(T).DeepClone(instance);

		private static readonly MethodInfo RuntimeHelpers_GetUninitializedObject = typeof(RuntimeHelpers).GetMethod("GetUninitializedObject", BindingFlags.Public | BindingFlags.Static)!;
		private static readonly MethodInfo Object_GetType = typeof(object).GetMethod("GetType", BindingFlags.Public | BindingFlags.Instance)!;

		private static readonly MethodInfo FastReflection_CloneObject = typeof(FastReflection).GetMethod(nameof(CloneObject), BindingFlags.NonPublic | BindingFlags.Static)!;
		private static readonly MethodInfo FastReflection_DefaultObj = typeof(FastReflection).GetMethod(nameof(DefaultObj), BindingFlags.NonPublic | BindingFlags.Static)!;
		private static readonly MethodInfo FastReflection_UninitializedObject = typeof(FastReflection).GetMethod(nameof(UninitializedObject), BindingFlags.NonPublic | BindingFlags.Static)!;
		private static readonly MethodInfo FastReflection_UnsafeInit = typeof(FastReflection).GetMethod(nameof(UnsafeInit), BindingFlags.NonPublic | BindingFlags.Static)!;
		private static readonly MethodInfo FastReflection_ActivatorObject = typeof(FastReflection).GetMethod(nameof(ActivatorObject), BindingFlags.NonPublic | BindingFlags.Static)!;
		private static readonly MethodInfo FastReflection_CloneArray = typeof(FastReflection).GetMethod(nameof(CloneArray), BindingFlags.NonPublic | BindingFlags.Static)!;
		private static readonly MethodInfo FastReflection_ObjectDefinedAlready = typeof(FastReflection).GetMethod(nameof(ObjectDefinedAlready), BindingFlags.NonPublic | BindingFlags.Static)!;

		private static Func<object?, object?> BuildDeepCloneDelegate(Type type) {
			if (cloneFuncs.TryGetValue(type, out var func))
				return func;

			string name = $"{typeof(FastReflection).FullName}.BuildDeepCloneDelegate<{type.GetSimplifiedGenericTypeName()}>.DeepClone";
			DynamicMethod method = new(name, typeof(object), new[] { typeof(object) }, typeof(FastReflection).Module, skipVisibility: true);

			ILGenerator il = method.GetILGenerator();

			// Optimization:  If the type is a struct and every single member (and their members) are unmanaged,
			// then this object is also unmanaged and can just be copied directly.
			// This check will also cover the primitive types.  Nice!
			bool hasRefs = BuildTypeHasRefsDelegate(type).Invoke();
			if (!hasRefs) {
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ret);
				goto skipCloneLocalReturn;
			}

			// Optimization/Bug Fix:  If the type is an array, then execute the CloneArray helper
			if (typeof(Array).IsAssignableFrom(type)) {
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Unbox_Any, type);
				il.Emit(OpCodes.Call, FastReflection_CloneArray.MakeGenericMethod(type.GetElementType()!));
				il.Emit(OpCodes.Box, type);
				il.Emit(OpCodes.Ret);
				goto skipCloneLocalReturn;
			}
			
			// Optimization/Bug Fix:  If the type is a string, then just return the string.  Copying the fields doesn't suffice
			if (type == typeof(string)) {
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ret);
				goto skipCloneLocalReturn;
			}

			// Optimization:  If the argument is null, then the clone will also be null
			Label afterNullCheck = il.DefineLabel();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Bne_Un, afterNullCheck);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Ret);
			il.MarkLabel(afterNullCheck);

			// Optimization/Bug Fix:  If an object was already cloned, just return that object in order to prevent infinite recursion
			Label afterRecursionCheck = il.DefineLabel();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, FastReflection_ObjectDefinedAlready);
			il.Emit(OpCodes.Brfalse, afterRecursionCheck);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ret);
			il.MarkLabel(afterRecursionCheck);

			var obj = il.DeclareLocal(type);
			var clone = il.DeclareLocal(type);
			var uninitSuccess = il.DeclareLocal(typeof(bool));
			
			// Copy the argument to the local
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Unbox_Any, type);
			il.Emit(OpCodes.Stloc, obj);

			// Initialize the clone
			// If initializing the clone fails via "UninitializedObject<T>", then bail
			Label skipBailDueToInvalidType = il.DefineLabel();
			il.Emit(OpCodes.Ldloca, uninitSuccess);
			il.Emit(OpCodes.Call, FastReflection_UninitializedObject.MakeGenericMethod(type));
			il.Emit(OpCodes.Stloc, clone);
			il.Emit(OpCodes.Ldloc, uninitSuccess);
			il.Emit(OpCodes.Brtrue, skipBailDueToInvalidType);
			il.Emit(OpCodes.Ldloc, clone);
			il.Emit(OpCodes.Ret);
			il.MarkLabel(skipBailDueToInvalidType);

			// Type has at least one reference type somewhere...  everything needs to be cloned manually
			if (!cachedFieldInfoArrays.TryGetValue(type, out var fields))
				cachedFieldInfoArrays[type] = fields = type.GetFields(AllFlags & (~BindingFlags.Static)).Where(f => !f.IsStatic && !f.IsLiteral).ToArray();

			// Do the mario
			var temp = il.DeclareLocal(typeof(object));
			foreach (var field in fields) {
				il.Emit(type.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc, obj);
				il.Emit(OpCodes.Ldfld, field);
				il.Emit(OpCodes.Box, field.FieldType);

				il.Emit(OpCodes.Call, FastReflection_CloneObject);

				il.Emit(OpCodes.Stloc, temp);

				il.Emit(type.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc, clone);
				il.Emit(OpCodes.Ldloc, temp);

				if (field.FieldType.IsValueType) {
					Label afterFieldNullCheck = il.DefineLabel();
					Label fieldAssignment = il.DefineLabel();

					il.Emit(OpCodes.Isinst, field.FieldType);
					il.Emit(OpCodes.Brfalse, afterFieldNullCheck);

					il.Emit(OpCodes.Ldloc, temp);
					il.Emit(OpCodes.Unbox_Any, field.FieldType);
					il.Emit(OpCodes.Br, fieldAssignment);
					il.MarkLabel(afterFieldNullCheck);

					// Return value was null.  Use "DefaultObj<T>()" to get a default object per type
					il.Emit(OpCodes.Call, FastReflection_DefaultObj.MakeGenericMethod(field.FieldType));
					il.MarkLabel(fieldAssignment);
				} else {
					il.Emit(OpCodes.Isinst, field.FieldType);
				}

				il.Emit(OpCodes.Stfld, field);
			}

			// If no fields are present, then an uninitialized object is the same as a cloned object, so this should be fine
			il.Emit(OpCodes.Ldloc, clone);
			il.Emit(OpCodes.Box, type);
			il.Emit(OpCodes.Ret);

			skipCloneLocalReturn:

			cloneFuncs[type] = func = method.CreateDelegate<Func<object?, object?>>();

			return func;
		}

		private static readonly Dictionary<Type, Func<bool>> typeHasRefs = new();
		
		private static Func<bool> BuildTypeHasRefsDelegate(Type type) {
			if (typeHasRefs.TryGetValue(type, out var func))
				return func;

			string name = $"{typeof(FastReflection).FullName}.BuildTypeHasRefsDelegate<{type.GetSimplifiedGenericTypeName()}>";
			DynamicMethod method = new(name, typeof(bool), null, typeof(FastReflection).Module, skipVisibility: true);

			ILGenerator il = method.GetILGenerator();

			il.Emit(OpCodes.Ldsfld, typeof(TypeInfo<>).MakeGenericType(type).GetField("IsRefOrHasRefs", BindingFlags.Public | BindingFlags.Static)!);
			il.Emit(OpCodes.Ret);

			typeHasRefs[type] = func = method.CreateDelegate<Func<bool>>();

			return func;
		}

		private static bool ObjectDefinedAlready(object? obj) {
			if (obj is null || !BuildTypeHasRefsDelegate(obj.GetType()).Invoke())
				return false;

			return DetectedObjectInstances.Contains(obj);
		}

		private static object? CloneObject(object? obj) => obj?.GetType().DeepClone(obj);

		private static T?[]? CloneArray<T>(T[]? array) {
			if (array is null)
				return null;
			
			T?[] ret = new T[array.Length];
			
			for (int i = 0; i < array.Length; i++)
				ret[i] = (T?)CloneObject(array[i]);
			
			return ret;
		}

		private static T? DefaultObj<T>() => default;

		private static T? UninitializedObject<T>(out bool success) {
			// Types are manually specified here based on the CLR source code
			// https://github.com/dotnet/runtime/blob/4cbe6f99d23e04c56a89251d49de1b0f14000427/src/coreclr/vm/reflectioninvocation.cpp#L1617
			// https://github.com/dotnet/runtime/blob/4cbe6f99d23e04c56a89251d49de1b0f14000427/src/coreclr/vm/reflectioninvocation.cpp#L1865
			Type check = typeof(T);
			success = true;

			// Don't allow void
			if (check == typeof(void))
				throw new InvalidGenericTypeException("void");

			// Don't allow generic variables (e.g., the 'T' from List<T>) or open generic types (List<>)
			if (check.IsGenericTypeParameter || check.IsGenericTypeDefinition)
				throw new InvalidGenericTypeException(check);

			// Don't allow arrays, pointers, byrefs, or function pointers
			if (check.IsPointer) {
				success = false;
				return default;
			}

			// Don't allow ref structs
			if (check.IsByRefLike) {
				success = false;
				return default;
			}

			// Don't allow abstract classes or interface types
			if (check.IsAbstract) {
				success = false;
				return default;
			}

			if (check.IsByRef)
				return UnsafeInit<T>();

			// Don't allow creating instances of delegates
			if (typeof(Delegate).IsAssignableFrom(check))
				return UnsafeInit<T>();

			// Don't allow string or string-like (variable length) types.
			if (TypeHasComponentSize<T>())
				return (T?)ActivatorObject(typeof(T));

			// The CLR source includes the following check, but "__Canon" has no meaning in C# land so it's probably safe to ignore
			/*
			// Don't allow generics instantiated over __Canon
			if (pMT->IsSharedByGenericInstantiations())
			{
				COMPlusThrow(kNotSupportedException, W("NotSupported_Type"));
			}
			*/

			// Yet another check from the CLR source that wouldn't make sense in C# land
			/*
			// Also do not allow allocation of uninitialized RCWs (COM objects).
			if (pMT->IsComObjectType())
				COMPlusThrow(kNotSupportedException, W("NotSupported_ManagedActivation"));
			*/

			// All of the checks have passed.  Make the object
			return GetUninitializedObject<T>();
		}

		// Prevent calling the static ctor too early
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static bool TypeHasComponentSize<T>() {
			// For some reason, just checking HasComponentSize won't work for strings... so they need to be manually specified
			return UnsafeHelper<T>.HasComponentSize || typeof(T) == typeof(string) || typeof(Array).IsAssignableFrom(typeof(T));
		}
		
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static T? GetUninitializedObject<T>() => (T?)RuntimeHelpers.GetUninitializedObject(typeof(T));

		private static T? UnsafeInit<T>() {
			Unsafe.SkipInit(out T value);
			return value;
		}

		private static object? ActivatorObject(Type type) {
			if (type == typeof(string))
				return string.Empty;
			
			if (type.IsValueType || type.GetConstructor(Type.EmptyTypes) is not null)
				return Activator.CreateInstance(type);

			return BuildActivatorDelegate(type).Invoke();
		}

		private static readonly Dictionary<Type, Func<object>> activatorObjectDelegate = new();

		private static Func<object> BuildActivatorDelegate(Type type) {
			if (activatorObjectDelegate.TryGetValue(type, out var func))
				return func;

			string name = $"{typeof(FastReflection).FullName}.BuildActivatorDelegate<{type.GetSimplifiedGenericTypeName()}>";
			DynamicMethod method = new(name, typeof(object), null, typeof(FastReflection).Module, skipVisibility: true);

			ILGenerator il = method.GetILGenerator();

			il.Emit(OpCodes.Call, FastReflection_UnsafeInit.MakeGenericMethod(type));
			il.Emit(OpCodes.Box, type);
			il.Emit(OpCodes.Ret);

			activatorObjectDelegate[type] = func = method.CreateDelegate<Func<object>>();

			return func;
		}

		private static class TypeInfo<T> {
			public static readonly bool IsRefOrHasRefs = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
		}
		
		// The following lines contain a collection of methods/data/etc. to make "UninitializedObject<T>()" work in line with the internal CLR code
		private static readonly MethodInfo RuntimeHelpers_GetMethodTable = typeof(RuntimeHelpers).GetMethod("GetMethodTable", BindingFlags.NonPublic | BindingFlags.Static)!;
		private static readonly Type Type_MethodTable = Type.GetType("System.Runtime.CompilerServices.MethodTable")!;
		private static readonly MethodInfo Type_GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static)!;
		private static readonly MethodInfo GC_KeepAlive = typeof(GC).GetMethod("KeepAlive", BindingFlags.Public | BindingFlags.Static)!;

		private static readonly MethodInfo FastReflection_RetrieveField_T = typeof(FastReflection).GetMethod(nameof(RetrieveField), 1, BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Type), typeof(string), typeof(object) }, null)!;
		private static readonly MethodInfo FastReflection_RetrieveField_uint = FastReflection_RetrieveField_T.MakeGenericMethod(typeof(uint));
		
		private static class UnsafeHelper<T> {
			private const uint enum_flag_HasComponentSize = 0x80000000;

			public static readonly bool HasComponentSize = BuildHasComponentSizeDelegate().Invoke();

			private static Func<bool> BuildHasComponentSizeDelegate() {
				string name = $"{typeof(FastReflection).FullName}/{nameof(UnsafeHelper<T>)}<{typeof(T).GetSimplifiedGenericTypeName()}>.BuildHasComponentSizeDelegate";
				DynamicMethod method = new(name, typeof(bool), null, typeof(FastReflection).Module, skipVisibility: true);

				ILGenerator il = method.GetILGenerator();
				var obj = il.DeclareLocal(typeof(object));

				// Load the type and field name for later use in FastReflection.RetrieveField
				il.Emit(OpCodes.Ldtoken, Type_MethodTable);
				il.Emit(OpCodes.Call, Type_GetTypeFromHandle);

				il.Emit(OpCodes.Ldstr, "Flags");

				// Making a default object should be fine since this delegate is only used once
				il.Emit(OpCodes.Ldtoken, typeof(T));
				il.Emit(OpCodes.Call, Type_GetTypeFromHandle);
				il.Emit(OpCodes.Call, FastReflection_ActivatorObject);
				il.Emit(OpCodes.Stloc, obj);
				il.Emit(OpCodes.Ldloc, obj);
				// Get the method table
				il.Emit(OpCodes.Call, RuntimeHelpers_GetMethodTable);
				il.Emit(OpCodes.Ldobj, Type_MethodTable);
				il.Emit(OpCodes.Box, Type_MethodTable);

				// Extract the field
				il.Emit(OpCodes.Call, FastReflection_RetrieveField_uint);

				// Unbox the value and perform the arithmetic:
				// (Flags & enum_flag_HasComponentSize) != 0
				il.Emit(OpCodes.Ldc_I4, unchecked ((int)enum_flag_HasComponentSize));
				il.Emit(OpCodes.And);

				// Compare the result to 0 and make the delegate return true if they aren't equal
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Cgt_Un);

				// Ensure that the object that the method table was retrieved from can't be destroyed too early
				il.Emit(OpCodes.Ldloc, obj);
				il.Emit(OpCodes.Call, GC_KeepAlive);

				il.Emit(OpCodes.Ret);

				return method.CreateDelegate<Func<bool>>();
			}
		}
	}
}
