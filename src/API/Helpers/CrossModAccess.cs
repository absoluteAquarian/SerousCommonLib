using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;

namespace SerousCommonLib.API {
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

		public static Type GetTypeFromAssembly(this Mod mod, string fullName) {
			string key = mod.Name + ":" + fullName;

			if (!typeCache.TryGetValue(key, out Type type))
				typeCache[key] = type = mod.Code.GetType(fullName) ?? throw new Exception($"Type \"{fullName}\" does not exist in mod \"{mod.Name}\"");

			return type;
		}

		public const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

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

		public static T FindAndInvoke<T>(this Type type, string method, Type[] argTypes, object instance, params object[] args) {
			MethodInfo methodInfo = GetMethod(type, method, argTypes, out string key);

			if (!typeof(T).IsAssignableFrom(methodInfo.ReturnType))
				throw new Exception($"The return type for method \"{key}\" cannot be cast to type \"{typeof(T).GetSimplifiedGenericTypeName()}\"");

			return (T)methodInfo.Invoke(instance, args);
		}

		public static object FindAndInvoke(this Type type, string method, Type[] argTypes, object instance, params object[] args) {
			MethodInfo methodInfo = GetMethod(type, method, argTypes, out _);
				
			return methodInfo.Invoke(instance, args);
		}

		public static FieldInfo GetField(Type type, string field, out string key) {
			key = type.FullName + "::" + field;

			if (!fieldCache.TryGetValue(key, out var fieldInfo))
				fieldCache[key] = fieldInfo = type.GetField(field, AllFlags) ?? throw new Exception($"Field \"{field}\" could not be found in type \"{type.GetSimplifiedGenericTypeName()}\"");

			return fieldInfo;
		}

		public static T FindAndGet<T>(this Type type, string field, object instance) {
			FieldInfo fieldInfo = GetField(type, field, out string key);
			
			if (!typeof(T).IsAssignableFrom(fieldInfo.FieldType))
				throw new Exception($"Field \"{key}\" cannot be cast to type \"{type.GetSimplifiedGenericTypeName()}\"");

			return (T)fieldInfo.GetValue(instance);
		}

		public static object FindAndGet(this Type type, string field, object instance) {
			FieldInfo fieldInfo = GetField(type, field, out _);

			return fieldInfo.GetValue(instance);
		}

		public static void FindAndSet(this Type type, string field, object instance, object value) {
			FieldInfo fieldInfo = GetField(type, field, out _);

			fieldInfo.SetValue(instance, value);
		}
	}
}
