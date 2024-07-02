using System;

#nullable enable
namespace SerousCommonLib.API {
	partial class Extensions {
		/// <summary>
		/// Creates a new weak reference to the specified object, or <see langword="null"/> if the object is <see langword="null"/>.
		/// </summary>
		public static WeakReference<T>? AsWeakReference<T>(this T? self) where T : class => self is null ? null : new WeakReference<T>(self);
	}
}
