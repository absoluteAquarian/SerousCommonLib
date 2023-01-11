using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;

namespace SerousCommonLib.API {
	/// <summary>
	/// A helper class for accessing/setting information in other mods
	/// </summary>
	public static class CrossModAccess {
		internal class Loadable : ILoadable {
			public void Load(Mod mod) {
				methodCache = new();
				fieldCache = new();
				typeCache = new();
			}

			public void Unload() {
				methodCache = null;
				fieldCache = null;
				typeCache = null;
			}
		}

		private static Dictionary<string, MethodInfo> methodCache;
		private static Dictionary<string, FieldInfo> fieldCache;
		private static Dictionary<string, Type> typeCache;

		/// <summary>
		/// Attempts to retrieve a type whose name is <paramref name="fullName"/> from <paramref name="mod"/>'s assembly<br/>
		/// This method will throw an exception if the type does not exist
		/// </summary>
		/// <param name="mod">The mod instance</param>
		/// <param name="fullName">The full <c>namespace.class</c> name of the type</param>
		/// <returns>The found type</returns>
		/// <exception cref="Exception"/>
		public static Type GetTypeFromAssembly(this Mod mod, string fullName) {
			string key = mod.Name + ":" + fullName;

			if (!typeCache.TryGetValue(key, out Type type))
				typeCache[key] = type = mod.Code.GetType(fullName) ?? throw new Exception($"Type \"{fullName}\" does not exist in mod \"{mod.Name}\"");

			return type;
		}

		public const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		/// <summary>
		/// Retrieves a <see cref="MethodInfo"/> representing a method from a type<br/>
		/// This method will throw an exception if the method does not exist
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="method">The name of the method</param>
		/// <param name="argTypes">An array of the method's argument types, or <see langword="null"/> for no arguments</param>
		/// <param name="key">A unique identifier for the <see cref="MethodInfo"/> object</param>
		/// <returns>The found method</returns>
		/// <exception cref="Exception"/>
		public static MethodInfo GetMethod(Type type, string method, Type[] argTypes, out string key) {
			key = type.FullName + "::" + method;

			if (argTypes?.Length > 0)
				key += "(" + string.Join(',', argTypes.Select(Extensions.GetSimplifiedGenericTypeName)) + ")";
			else
				key += "()";

			if (!methodCache.TryGetValue(key, out var methodInfo)) {
				methodCache[key] = methodInfo = (argTypes is not null
					? type.GetMethod(method, AllFlags, argTypes)
					: type.GetMethod(method, AllFlags))
					?? throw new Exception($"Method \"{key}\" could not be found in type \"{type.GetSimplifiedGenericTypeName()}\"");
			}

			return methodInfo;
		}

		/// <summary>
		/// Attempts to find a method within <paramref name="type"/>, invokes it then returns its return value casted to <typeparamref name="T"/><br/>
		/// This method will throw an exception if either the method does not exist or its return value cannot be cast to <typeparamref name="T"/>
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="method">The name of the method</param>
		/// <param name="argTypes">An array of the method's argument types, or <see langword="null"/> for no arguments</param>
		/// <param name="instance">The instance to call the method on, or <see langword="null"/> if the method is static</param>
		/// <param name="args">An array of arguments passed to the method</param>
		/// <returns>The return value from the method</returns>
		/// <exception cref="Exception"/>
		public static T FindAndInvoke<T>(this Type type, string method, Type[] argTypes, object instance, params object[] args) {
			MethodInfo methodInfo = GetMethod(type, method, argTypes, out string key);

			if (!typeof(T).IsAssignableFrom(methodInfo.ReturnType))
				throw new Exception($"The return type for method \"{key}\" cannot be cast to type \"{typeof(T).GetSimplifiedGenericTypeName()}\"");

			return (T)methodInfo.Invoke(instance, args);
		}

		/// <summary>
		/// Attempts to find a method within <paramref name="type"/>, invokes it then returns its return value
		/// This method will throw an exception if the method does not exist
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="method">The name of the method</param>
		/// <param name="argTypes">An array of the method's argument types, or <see langword="null"/> for no arguments</param>
		/// <param name="instance">The instance to call the method on, or <see langword="null"/> if the method is static</param>
		/// <param name="args">An array of arguments passed to the method</param>
		/// <returns>The return value from the method</returns>
		/// <exception cref="Exception"/>
		public static object FindAndInvoke(this Type type, string method, Type[] argTypes, object instance, params object[] args) {
			MethodInfo methodInfo = GetMethod(type, method, argTypes, out _);
				
			return methodInfo.Invoke(instance, args);
		}

		/// <summary>
		/// Retrieves a <see cref="FieldInfo"/> representing a field from <paramref name="type"/><br/>
		/// This method will throw an exception if the field does not exist
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="field">The name of the field</param>
		/// <param name="key">A unique identifier for the <see cref="FieldInfo"/> object</param>
		/// <returns>The found field</returns>
		/// <exception cref="Exception"/>
		public static FieldInfo GetField(Type type, string field, out string key) {
			key = type.FullName + "::" + field;

			if (!fieldCache.TryGetValue(key, out var fieldInfo))
				fieldCache[key] = fieldInfo = type.GetField(field, AllFlags) ?? throw new Exception($"Field \"{field}\" could not be found in type \"{type.GetSimplifiedGenericTypeName()}\"");

			return fieldInfo;
		}

		/// <summary>
		/// Attempts to find a field within <paramref name="type"/>, retrieves its value and then casts it to <typeparamref name="T"/><br/>
		/// This method will throw an exception if either the field does not exist or it cannot be cast to <typeparamref name="T"/>
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="field">The name of the field</param>
		/// <param name="instance">The instance to retrieve the field from, or <see langword="null"/> if the field is static</param>
		/// <returns>The field's value</returns>
		/// <exception cref="Exception"/>
		public static T FindAndGet<T>(this Type type, string field, object instance) {
			FieldInfo fieldInfo = GetField(type, field, out string key);
			
			if (!typeof(T).IsAssignableFrom(fieldInfo.FieldType))
				throw new Exception($"Field \"{key}\" cannot be cast to type \"{type.GetSimplifiedGenericTypeName()}\"");

			return (T)fieldInfo.GetValue(instance);
		}

		/// <summary>
		/// Attempts to find a field within <paramref name="type"/> and retrieves its value<br/>
		/// This method will throw an exception if the field does not exist
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="field">The name of the field</param>
		/// <param name="instance">The instance to retrieve the field from, or <see langword="null"/> if the field is static</param>
		/// <returns>The field's value</returns>
		/// <exception cref="Exception"/>
		public static object FindAndGet(this Type type, string field, object instance) {
			FieldInfo fieldInfo = GetField(type, field, out _);

			return fieldInfo.GetValue(instance);
		}

		/// <summary>
		/// Attempts to find a field within <paramref name="type"/> and assign it a value<br/>
		/// This method will throw an exception if either the field does not exist or the value cannot be assigned to the field
		/// </summary>
		/// <param name="type">The type</param>
		/// <param name="field">The name of the field</param>
		/// <param name="instance">The instance to retrieve the field from, or <see langword="null"/> if the field is static</param>
		/// <param name="value">The value to assign to the field</param>
		/// <exception cref="Exception"/>
		public static void FindAndSet(this Type type, string field, object instance, object value) {
			FieldInfo fieldInfo = GetField(type, field, out _);

			fieldInfo.SetValue(instance, value);
		}
	}
}
