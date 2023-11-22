using System.Reflection;
using System.Runtime.Loader;
using System;

#nullable enable
namespace SerousCommonLib.API {
	internal static class ALCReflectionUnloader {
		public static void OnUnload(Assembly assembly, Action action) {
			AssemblyLoadContext? alc = AssemblyLoadContext.GetLoadContext(assembly);

			if (alc is not null)
				alc.Unloading += _ => action();
		}
	}
}
