using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace SerousCommonLib.API {
	partial class Extensions {
		public static int MakeLocalVariable<T>(this ILContext il) {
			var def = new VariableDefinition(il.Import(typeof(T)));
			il.Body.Variables.Add(def);
			return def.Index;
		}
	}
}
