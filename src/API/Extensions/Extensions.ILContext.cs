using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace SerousCommonLib.API {
	partial class Extensions {
		/// <summary>
		/// Defines a local variable
		/// </summary>
		/// <param name="il">The context</param>
		/// <returns>The index of the local variable in the locals table</returns>
		public static int MakeLocalVariable<T>(this ILContext il) {
			var def = new VariableDefinition(il.Import(typeof(T)));
			il.Body.Variables.Add(def);
			return def.Index;
		}
	}
}
