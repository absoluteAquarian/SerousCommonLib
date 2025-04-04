using Mono.Cecil.Cil;
using MonoMod.Cil;
using SerousCommonLib.UI;
using System;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace SerousCommonLib.API.Edits {
	internal class ClippedListRecalculateSupport : Edit {
		public override void LoadEdits() {
			IL_UIElement.Recalculate += Patch_UIElement_Recalculate;
		}

		public override void UnloadEdits() {
			IL_UIElement.Recalculate -= Patch_UIElement_Recalculate;
		}

		private static void Patch_UIElement_Recalculate(ILContext il) => ILHelper.CommonPatchingWrapper(il, SerousUtilities.Instance, InjectAdditionalTypeChecks);
		
		private static bool InjectAdditionalTypeChecks(ILCursor c, ref string badReturnReason) {
			/*   IL_0028: ldarg.0
			 *   IL_0029: call instance class Terraria.UI.UIElement Terraria.UI.UIElement::get_Parent()
			 *   IL_002e: isinst Terraria.GameContent.UI.Elements.UIList
			 * [ IL_0033: brfalse.s IL_0041  ===>  brfalse.s IL_XXX1 ]
			 *   IL_0035: ldloca.s 0
			 *   IL_0037: ldc.r4 3.4028235E+38
			 *   IL_003c: stfld float32 Terraria.UI.CalculatedStyle::Height
			 * [ IL_XXX0: br IL_0041 ]
			 * [ IL_XXX1: ldarg.0 ]
			 * [ IL_XXX2: call instance class Terraria.UI.UIElement Terraria.UI.UIElement::get_Parent() ]
			 * [ IL_XXX3: isinst SerousCommonLib.UI.BaseClippedList ]
			 * [ IL_XXX4: brtrue.s IL_0035 ]
			 *   IL_0041: ldarg.0
			 *   IL_0042: ldarg.0
			 */
			MethodInfo UIElement_get_Parent = typeof(UIElement).GetProperty(nameof(UIElement.Parent)).GetGetMethod();

			ILLabel afterBlock = null;
			if (!c.TryGotoNext(MoveType.After,
				i => i.MatchLdarg0(),
				i => i.MatchCall(UIElement_get_Parent),
				i => i.MatchIsinst(typeof(UIList)),
				i => i.MatchBrfalse(out afterBlock))) {
				badReturnReason = "Failed to find the UIList check";
				return false;
			}

			if (!c.TryGotoNext(MoveType.Before,
				i => i.MatchLdloca(0),
				i => i.MatchLdcR4(float.MaxValue),
				i => i.MatchStfld(out _))) {
				badReturnReason = "Failed to find local height assignment";
				return false;
			}

			ILLabel heightAssignment = c.DefineLabel();
			heightAssignment.Target = c.Next;

			if (!c.TryGotoNext(MoveType.Before,
				i => i.MatchLdarg0(),
				i => i.MatchLdarg0())) {
				badReturnReason = "Failed to find assignment for _outerDimensions";
				return false;
			}

			// Inject a jump to the type checks
			ILLabel afterHeightAssignment = c.DefineLabel();
			afterHeightAssignment.Target = afterBlock.Target;
			c.Emit(OpCodes.Br, afterHeightAssignment);

			// In order to preserve the original IL, these checks are not placed next to the original ones
			c.Emit(OpCodes.Ldarg_0);
			afterBlock.Target = c.Prev;  // Overwrite the branch to jump to this injection instead
			c.Emit(OpCodes.Call, UIElement_get_Parent);
			c.Emit(OpCodes.Isinst, typeof(BaseClippedList));
			c.Emit(OpCodes.Brtrue, heightAssignment);

			return true;
		}
	}
}
