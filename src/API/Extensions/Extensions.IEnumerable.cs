using SerousCommonLib.API.Iterators;
using System.Collections.Generic;
using Terraria;

namespace SerousCommonLib.API {
	partial class Extensions {
		/// <summary>
		/// Iterates through <paramref name="source"/> randomly without repetitions
		/// </summary>
		public static IEnumerable<T> IterateRandomly<T>(this IEnumerable<T> source) => new RandomEnumerableIterator<T>(source, Main.rand.Next, false);
	}
}
