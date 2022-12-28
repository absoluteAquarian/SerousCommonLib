using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SerousCommonLib.API {
	partial class Extensions {
		/// <summary>
		/// Enumerates all exception handlers whose catch/finally clause end points to the current instruction (<c>handler.HandlerEnd == Next</c>)
		/// </summary>
		public static IEnumerable<ExceptionHandler> IncomingHandlers(this ILCursor c)
			=> !c.Body.HasExceptionHandlers ? Array.Empty<ExceptionHandler>() : c.Body.ExceptionHandlers.Where(e => e.HandlerEnd == c.Next);

		public static void EmitIfBlock(this ILCursor c, out ILLabel targetAfterBlock, Func<bool> condition, Action<ILCursor> action, params ILLabel[] targetsToUpdate)
			=> c.EmitIfBlock(out targetAfterBlock, condition, action, targetsToUpdate as IEnumerable<ILLabel>);

		public static void EmitIfBlock(this ILCursor c, out ILLabel targetAfterBlock, Func<bool> condition, Action<ILCursor> action, IEnumerable<ILLabel> targetsToUpdate = null) {
			targetAfterBlock = c.DefineLabel();

			int index = c.Index;
			// if (condition) {
			c.EmitDelegate(condition);
			c.Emit(OpCodes.Brfalse, targetAfterBlock);
			action(c);
			// }

			if (targetsToUpdate is not null) {
				Instruction target = c.Instrs[index];

				foreach (ILLabel targetToUpdate in targetsToUpdate)
					targetToUpdate.Target = target;
			}

			targetAfterBlock.Target = c.Next;
		}

		public static void EmitElseIfBlock(this ILCursor c, out ILLabel targetAfterIfBlock, ILLabel targetAfterEverything, Func<bool> condition, Action<ILCursor> action, params ILLabel[] targetsToUpdate)
			=> c.EmitElseIfBlock(out targetAfterIfBlock, targetAfterEverything, condition, action, targetsToUpdate as IEnumerable<ILLabel>);

		public static void EmitElseIfBlock(this ILCursor c, out ILLabel targetAfterIfBlock, ILLabel targetAfterEverything, Func<bool> condition, Action<ILCursor> action, IEnumerable<ILLabel> targetsToUpdate = null) {
			c.EmitIfBlock(out targetAfterIfBlock, condition, action, targetsToUpdate);
			c.Emit(OpCodes.Br, targetAfterEverything);
		}

		public static void EmitIfElseBlock(this ILCursor c, out ILLabel targetAfterIfBlock, out ILLabel targetAfterEverything, Func<bool> condition, Action<ILCursor> actionWhenTrueCondition, Action<ILCursor> actionWhenFalseCondition, params ILLabel[] targetsToUpdate)
			=> c.EmitIfElseBlock(out targetAfterIfBlock, out targetAfterEverything, condition, actionWhenTrueCondition, actionWhenFalseCondition, targetsToUpdate as IEnumerable<ILLabel>);

		public static void EmitIfElseBlock(this ILCursor c, out ILLabel targetAfterIfBlock, out ILLabel targetAfterEverything, Func<bool> condition, Action<ILCursor> actionWhenTrueCondition, Action<ILCursor> actionWhenFalseCondition, IEnumerable<ILLabel> targetsToUpdate = null) {
			targetAfterEverything = c.DefineLabel();
			
			// if (condition) {
			c.EmitElseIfBlock(out targetAfterIfBlock, targetAfterEverything, condition, actionWhenTrueCondition, targetsToUpdate);
			// } else {
			int start = c.Index;
			actionWhenFalseCondition(c);
			// }

			// Mark the first label
			targetAfterIfBlock.Target = c.Instrs[start];
		}

		public static void EmitIfElseChainBlock(this ILCursor c, out ILLabel[] blockEndTargets, ILElseIfBlock[] blocks, Action<ILCursor> elseBlockAction, params ILLabel[] targetsToUpdate)
			=> c.EmitIfElseChainBlock(out blockEndTargets, blocks, elseBlockAction, targetsToUpdate as IEnumerable<ILLabel>);

		public static void EmitIfElseChainBlock(this ILCursor c, out ILLabel[] blockEndTargets, ILElseIfBlock[] blocks, Action<ILCursor> elseBlockAction, IEnumerable<ILLabel> targetsToUpdate = null) {
			blockEndTargets = new ILLabel[blocks.Length + 1];
			ILLabel targetAfterEverything = blockEndTargets[^1] = c.DefineLabel();

			// if (condition[0]) {
			var (condition, action) = blocks[0];
			c.EmitElseIfBlock(out blockEndTargets[0], targetAfterEverything, blocks[0].condition, blocks[0].action, targetsToUpdate);
			// }

			for (int i = 1; i < blocks.Length; i++) {
				// else if (condition[i]) {
				(condition, action) = blocks[i];
				c.EmitElseIfBlock(out blockEndTargets[i], targetAfterEverything, condition, action, blockEndTargets[i - 1]);
				// }
			}
			
			// else {
			if (elseBlockAction is not null) {
				int start = c.Index;
				elseBlockAction.Invoke(c);
				blockEndTargets[blocks.Length - 1].Target = c.Instrs[start];
			} else
				blockEndTargets[blocks.Length - 1] = targetAfterEverything;
			// }

			targetAfterEverything.Target = c.Next;
		}

		public static void EmitForLoop(this ILCursor c, Action<ILCursor> init, Action<ILCursor, ILLabel> condition, Action<ILCursor> step, Action<ILCursor> body) {
			ILLabel gotoStepCondition = c.DefineLabel();
			ILLabel gotoBodyStart = c.DefineLabel();
			
			// for (init; condition; step)

			// for (init;                )
			init(c);
			c.Emit(OpCodes.Br, gotoStepCondition);

			// loop body
			int bodyStart = c.Index;
			body(c);
			gotoBodyStart.Target = c.Instrs[bodyStart];

			// for (                 step)
			step(c);

			// for (      condition;     )
			int conditionStart = c.Index;
			condition(c, gotoBodyStart);
			gotoStepCondition.Target = c.Instrs[conditionStart];
		}

		public static void EmitSimpleForLoop(this ILCursor c, int start, int step, Func<int, bool> condition, Action<int> body) {
			int local = c.Context.MakeLocalVariable<int>();

			c.EmitForLoop(
				init: c => {
					c.Emit(OpCodes.Ldc_I4, start);
					c.Emit(OpCodes.Stloc, local);
				},
				condition: (c, bodyStart) => {
					c.Emit(OpCodes.Ldloc, local);
					c.EmitDelegate(condition);
					c.Emit(OpCodes.Brtrue, bodyStart);
				},
				step: c => {
					c.Emit(OpCodes.Ldloc, local);
					c.Emit(OpCodes.Ldc_I4, step);
					c.Emit(OpCodes.Add);
					c.Emit(OpCodes.Stloc, local);
				},
				body: c => {
					c.Emit(OpCodes.Ldloc, local);
					c.EmitDelegate(body);
				});
		}
	}
}
