using System;
using System.Reflection;
using Terraria.ModLoader;

namespace SerousCommonLib.API {
	/// <summary>
	/// A type specializing in faster access of hidden members within types
	/// </summary>
	public static partial class FastReflection {
		private class Loadable : ILoadable {
			public static event Action Unload;

			void ILoadable.Load(Mod mod) { }

			void ILoadable.Unload() {
				// Field caches
				getFieldFuncs.Clear();
				setFieldFuncs.Clear();
				cachedFieldInfos.Clear();

				// Cloning caches
				cloneFuncs.Clear();
				cachedFieldInfoArrays.Clear();

				Unload?.Invoke();
				Unload = null;
			}
		}

		/// <summary>
		/// Binding flags for any access level, instance or static
		/// </summary>
		public const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	}
}
