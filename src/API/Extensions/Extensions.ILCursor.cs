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

		/// <summary>
		/// Emits the IL equivalent of an "if" block
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="targetAfterBlock">A label representing the a branching label to the instruction after this "if" block</param>
		/// <param name="condition">The boolean condition within the "if" statement</param>
		/// <param name="action">Write instructions that would go inside the "if" block here</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "if" block</param>
		public static void EmitIfBlock(this ILCursor c, out ILLabel targetAfterBlock, Func<bool> condition, Action<ILCursor> action, params ILLabel[] targetsToUpdate)
			=> c.EmitIfBlock(out targetAfterBlock, null, condition, action, targetsToUpdate as IEnumerable<ILLabel>);

		/// <inheritdoc cref="EmitIfBlock(ILCursor, out ILLabel, Func{bool}, Action{ILCursor}, ILLabel[])"/>
		public static void EmitIfBlock(this ILCursor c, out ILLabel targetAfterBlock, Func<bool> condition, Action<ILCursor> action, IEnumerable<ILLabel> targetsToUpdate = null)
			=> c.EmitIfBlock(out targetAfterBlock, null, condition, action, targetsToUpdate);

		/// <summary>
		/// Emits the IL equivalent of an "if" block
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="targetAfterBlock">A label representing the a branching label to the instruction after this "if" block</param>
		/// <param name="preConditionAction">An optional function to invoke before emitting the delegate for <paramref name="condition"/></param>
		/// <param name="condition">The boolean condition within the "if" statement</param>
		/// <param name="action">Write instructions that would go inside the "if" block here</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "if" block</param>
		public static void EmitIfBlock(this ILCursor c, out ILLabel targetAfterBlock, Action<ILCursor> preConditionAction, Func<bool> condition, Action<ILCursor> action, params ILLabel[] targetsToUpdate)
			=> c.EmitIfBlock(out targetAfterBlock, preConditionAction, condition, action, targetsToUpdate as IEnumerable<ILLabel>);

		/// <inheritdoc cref="EmitIfBlock(ILCursor, out ILLabel, Action{ILCursor}, Func{bool}, Action{ILCursor}, ILLabel[])"/>
		public static void EmitIfBlock(this ILCursor c, out ILLabel targetAfterBlock, Action<ILCursor> preConditionAction, Func<bool> condition, Action<ILCursor> action, IEnumerable<ILLabel> targetsToUpdate = null) {
			targetAfterBlock = c.DefineLabel();

			int index = c.Index;

			preConditionAction?.Invoke(c);

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

		/// <summary>
		/// Emits the IL equivalent of an "if" block
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="targetAfterBlock">A label representing the a branching label to the instruction after this "if" block</param>
		/// <param name="condition">The delegate for emitting the "if" condition.  A <see langword="bool"/> value is expected to be on the stack after this delegate's instructions are executed</param>
		/// <param name="action">Write instructions that would go inside the "if" block here</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "if" block</param>
		public static void EmitIfBlock(this ILCursor c, out ILLabel targetAfterBlock, Action<ILCursor> condition, Action<ILCursor> action, params ILLabel[] targetsToUpdate)
			=> c.EmitIfBlock(out targetAfterBlock, condition, action, targetsToUpdate as IEnumerable<ILLabel>);
		
		/// <inheritdoc cref="EmitIfBlock(ILCursor, out ILLabel, Action{ILCursor}, Action{ILCursor}, ILLabel[])"/>
		public static void EmitIfBlock(this ILCursor c, out ILLabel targetAfterBlock, Action<ILCursor> condition, Action<ILCursor> action, IEnumerable<ILLabel> targetsToUpdate = null) {
			targetAfterBlock = c.DefineLabel();

			int index = c.Index;

			// if (condition) {
			condition(c);
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

		/// <summary>
		/// Emits the IL equivalent of an "else if" block
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="targetAfterIfBlock">A label representing the a branching label to the instruction after this "else if" block</param>
		/// <param name="targetAfterEverything">A label representing where to jump to after executing this "else if" block's instructions</param>
		/// <param name="condition">The boolean condition within the "else if" statement</param>
		/// <param name="action">Write instructions that would go inside the "else if" block here</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "else if" block</param>
		public static void EmitElseIfBlock(this ILCursor c, out ILLabel targetAfterIfBlock, ILLabel targetAfterEverything, Func<bool> condition, Action<ILCursor> action, params ILLabel[] targetsToUpdate)
			=> c.EmitElseIfBlock(out targetAfterIfBlock, targetAfterEverything, null, condition, action, targetsToUpdate as IEnumerable<ILLabel>);

		/// <inheritdoc cref="EmitElseIfBlock(ILCursor, out ILLabel, ILLabel, Func{bool}, Action{ILCursor}, ILLabel[])"/>
		public static void EmitElseIfBlock(this ILCursor c, out ILLabel targetAfterIfBlock, ILLabel targetAfterEverything, Func<bool> condition, Action<ILCursor> action, IEnumerable<ILLabel> targetsToUpdate = null)
			=> c.EmitElseIfBlock(out targetAfterIfBlock, targetAfterEverything, null, condition, action, targetsToUpdate);

		/// <summary>
		/// Emits the IL equivalent of an "else if" block
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="targetAfterIfBlock">A label representing the a branching label to the instruction after this "else if" block</param>
		/// <param name="targetAfterEverything">A label representing where to jump to after executing this "else if" block's instructions</param>
		/// <param name="preConditionAction">An optional function to invoke before emitting the delegate for <paramref name="condition"/></param>
		/// <param name="condition">The boolean condition within the "else if" statement</param>
		/// <param name="action">Write instructions that would go inside the "else if" block here</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "else if" block</param>
		public static void EmitElseIfBlock(this ILCursor c, out ILLabel targetAfterIfBlock, ILLabel targetAfterEverything, Action<ILCursor> preConditionAction, Func<bool> condition, Action<ILCursor> action, params ILLabel[] targetsToUpdate)
			=> c.EmitElseIfBlock(out targetAfterIfBlock, targetAfterEverything, preConditionAction, condition, action, targetsToUpdate as IEnumerable<ILLabel>);

		/// <inheritdoc cref="EmitElseIfBlock(ILCursor, out ILLabel, ILLabel, Action{ILCursor}, Func{bool}, Action{ILCursor}, ILLabel[])"/>
		public static void EmitElseIfBlock(this ILCursor c, out ILLabel targetAfterIfBlock, ILLabel targetAfterEverything, Action<ILCursor> conditionAction, Func<bool> condition, Action<ILCursor> action, IEnumerable<ILLabel> targetsToUpdate = null) {
			c.EmitIfBlock(out targetAfterIfBlock, conditionAction, condition, action, targetsToUpdate);
			c.Emit(OpCodes.Br, targetAfterEverything);
		}

		/// <summary>
		/// Emits the IL equivalent of an "else if" block
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="targetAfterIfBlock">A label representing the a branching label to the instruction after this "else if" block</param>
		/// <param name="targetAfterEverything">A label representing where to jump to after executing this "else if" block's instructions</param>
		/// <param name="condition">The delegate for emitting the "if" condition.  A <see langword="bool"/> value is expected to be on the stack after this delegate's instructions are executed</param>
		/// <param name="action">Write instructions that would go inside the "else if" block here</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "else if" block</param>
		public static void EmitElseIfBlock(this ILCursor c, out ILLabel targetAfterIfBlock, ILLabel targetAfterEverything, Action<ILCursor> condition, Action<ILCursor> action, params ILLabel[] targetsToUpdate)
			=> c.EmitElseIfBlock(out targetAfterIfBlock, targetAfterEverything, condition, action, targetsToUpdate as IEnumerable<ILLabel>);

		/// <inheritdoc cref="EmitElseIfBlock(ILCursor, out ILLabel, ILLabel, Action{ILCursor}, Action{ILCursor}, ILLabel[])"/>
		public static void EmitElseIfBlock(this ILCursor c, out ILLabel targetAfterIfBlock, ILLabel targetAfterEverything, Action<ILCursor> condition, Action<ILCursor> action, IEnumerable<ILLabel> targetsToUpdate = null) {
			c.EmitIfBlock(out targetAfterIfBlock, condition, action, targetsToUpdate);
			c.Emit(OpCodes.Br, targetAfterEverything);
		}

		/// <summary>
		/// Emits the IL equivalent of an "if - else" chain
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="targetAfterIfBlock">A label representing the a branching label to the instruction after the "if" block</param>
		/// <param name="targetAfterEverything">A label representing where to jump to after executing the "if" block's instructions</param>
		/// <param name="condition">The boolean condition within the "if" statement</param>
		/// <param name="actionWhenTrueCondition">Write instructions that would go inside the "if" block here</param>
		/// <param name="actionWhenFalseCondition">Write instructions that would go inside the "else" block here</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "if" block</param>
		public static void EmitIfElseBlock(this ILCursor c, out ILLabel targetAfterIfBlock, out ILLabel targetAfterEverything, Func<bool> condition, Action<ILCursor> actionWhenTrueCondition, Action<ILCursor> actionWhenFalseCondition, params ILLabel[] targetsToUpdate)
			=> c.EmitIfElseBlock(out targetAfterIfBlock, out targetAfterEverything, null, condition, actionWhenTrueCondition, actionWhenFalseCondition, targetsToUpdate as IEnumerable<ILLabel>);

		/// <inheritdoc cref="EmitIfElseBlock(ILCursor, out ILLabel, out ILLabel, Func{bool}, Action{ILCursor}, Action{ILCursor}, ILLabel[])"/>
		public static void EmitIfElseBlock(this ILCursor c, out ILLabel targetAfterIfBlock, out ILLabel targetAfterEverything, Func<bool> condition, Action<ILCursor> actionWhenTrueCondition, Action<ILCursor> actionWhenFalseCondition, IEnumerable<ILLabel> targetsToUpdate = null)
			=> c.EmitIfElseBlock(out targetAfterIfBlock, out targetAfterEverything, null, condition, actionWhenTrueCondition, actionWhenFalseCondition, targetsToUpdate);

		/// <summary>
		/// Emits the IL equivalent of an "if - else" chain
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="targetAfterIfBlock">A label representing the a branching label to the instruction after the "if" block</param>
		/// <param name="targetAfterEverything">A label representing where to jump to after executing the "if" block's instructions</param>
		/// <param name="preConditionAction">An optional function to invoke before emitting the delegate for <paramref name="condition"/></param>
		/// <param name="condition">The boolean condition within the "if" statement</param>
		/// <param name="actionWhenTrueCondition">Write instructions that would go inside the "if" block here</param>
		/// <param name="actionWhenFalseCondition">Write instructions that would go inside the "else" block here</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "if" block</param>
		public static void EmitIfElseBlock(this ILCursor c, out ILLabel targetAfterIfBlock, out ILLabel targetAfterEverything, Action<ILCursor> preConditionAction, Func<bool> condition, Action<ILCursor> actionWhenTrueCondition, Action<ILCursor> actionWhenFalseCondition, params ILLabel[] targetsToUpdate)
			=> c.EmitIfElseBlock(out targetAfterIfBlock, out targetAfterEverything, preConditionAction, condition, actionWhenTrueCondition, actionWhenFalseCondition, targetsToUpdate as IEnumerable<ILLabel>);

		/// <inheritdoc cref="EmitIfElseBlock(ILCursor, out ILLabel, out ILLabel, Action{ILCursor}, Func{bool}, Action{ILCursor}, Action{ILCursor}, ILLabel[])"/>
		public static void EmitIfElseBlock(this ILCursor c, out ILLabel targetAfterIfBlock, out ILLabel targetAfterEverything, Action<ILCursor> preConditionAction, Func<bool> condition, Action<ILCursor> actionWhenTrueCondition, Action<ILCursor> actionWhenFalseCondition, IEnumerable<ILLabel> targetsToUpdate = null) {
			targetAfterEverything = c.DefineLabel();
			
			// if (condition) {
			c.EmitElseIfBlock(out targetAfterIfBlock, targetAfterEverything, preConditionAction, condition, actionWhenTrueCondition, targetsToUpdate);
			// } else {
			int start = c.Index;
			actionWhenFalseCondition(c);
			// }

			// Mark the first label
			targetAfterIfBlock.Target = c.Instrs[start];
		}

		/// <summary>
		/// Emits the IL equivalent of an "if - else" chain
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="targetAfterIfBlock">A label representing the a branching label to the instruction after the "if" block</param>
		/// <param name="targetAfterEverything">A label representing where to jump to after executing the "if" block's instructions</param>
		/// <param name="condition">The delegate for emitting the "if" condition.  A <see langword="bool"/> value is expected to be on the stack after this delegate's instructions are executed</param>
		/// <param name="actionWhenTrueCondition">Write instructions that would go inside the "if" block here</param>
		/// <param name="actionWhenFalseCondition">Write instructions that would go inside the "else" block here</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "if" block</param>
		public static void EmitIfElseBlock(this ILCursor c, out ILLabel targetAfterIfBlock, out ILLabel targetAfterEverything, Action<ILCursor> condition, Action<ILCursor> actionWhenTrueCondition, Action<ILCursor> actionWhenFalseCondition, params ILLabel[] targetsToUpdate)
			=> c.EmitIfElseBlock(out targetAfterIfBlock, out targetAfterEverything, condition, actionWhenTrueCondition, actionWhenFalseCondition, targetsToUpdate as IEnumerable<ILLabel>);

		/// <inheritdoc cref="EmitIfElseBlock(ILCursor, out ILLabel, out ILLabel, Action{ILCursor}, Action{ILCursor}, Action{ILCursor}, ILLabel[])"/>
		public static void EmitIfElseBlock(this ILCursor c, out ILLabel targetAfterIfBlock, out ILLabel targetAfterEverything, Action<ILCursor> condition, Action<ILCursor> actionWhenTrueCondition, Action<ILCursor> actionWhenFalseCondition, IEnumerable<ILLabel> targetsToUpdate = null) {
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

		/// <summary>
		/// Emits the IL equivalent  of an "if - else if - else" chain
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="blockEndTargets">A list of branching labels pointing where code flow should move to next when the condition for a block in this chain fails</param>
		/// <param name="blocks">A list of structures representing the "if" and "else if" blocks in this chain</param>
		/// <param name="elseBlockAction">Write the instructions that would go inside the "else" block here, or set this parameter to <see langword="null"/> to not generate an "else" block</param>
		/// <param name="targetsToUpdate">A list of branching labels to update to point to the start of this "if" block</param>
		public static void EmitIfElseChainBlock(this ILCursor c, out ILLabel[] blockEndTargets, ILElseIfBlock[] blocks, Action<ILCursor> elseBlockAction, params ILLabel[] targetsToUpdate)
			=> c.EmitIfElseChainBlock(out blockEndTargets, blocks, elseBlockAction, targetsToUpdate as IEnumerable<ILLabel>);

		/// <inheritdoc cref="EmitIfElseChainBlock(ILCursor, out ILLabel[], ILElseIfBlock[], Action{ILCursor}, ILLabel[])"/>
		public static void EmitIfElseChainBlock(this ILCursor c, out ILLabel[] blockEndTargets, ILElseIfBlock[] blocks, Action<ILCursor> elseBlockAction, IEnumerable<ILLabel> targetsToUpdate = null) {
			blockEndTargets = new ILLabel[blocks.Length + 1];
			ILLabel targetAfterEverything = blockEndTargets[^1] = c.DefineLabel();

			// if (condition[0]) {
			var (preConditionAction, condition, conditionDirect, action) = blocks[0];
			if (conditionDirect is not null)
				c.EmitElseIfBlock(out blockEndTargets[0], targetAfterEverything, conditionDirect, action, targetsToUpdate);
			else
				c.EmitElseIfBlock(out blockEndTargets[0], targetAfterEverything, preConditionAction, condition, action, targetsToUpdate);
			// }

			for (int i = 1; i < blocks.Length; i++) {
				// else if (condition[i]) {
				(preConditionAction, condition, conditionDirect, action) = blocks[i];
				if (conditionDirect is not null)
					c.EmitElseIfBlock(out blockEndTargets[i], targetAfterEverything, conditionDirect, action, blockEndTargets[i - 1]);
				else
					c.EmitElseIfBlock(out blockEndTargets[i], targetAfterEverything, preConditionAction, condition, action, blockEndTargets[i - 1]);
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

		/// <summary>
		/// Emits the IL equivalent of a "for (start; condition; step)" block
		/// </summary>
		/// <param name="c">The cursor</param>
		/// <param name="init">Write the instructions that should go in the loop initialization here</param>
		/// <param name="condition">Write the instructions that should go in the loop condition here.  The second argument is the branching label pointing to the start of <paramref name="body"/></param>
		/// <param name="step">Write the instructions that should go in the loop step here</param>
		/// <param name="body">Write instructions that should go in the body of the loop here</param>
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

		/// <summary>
		/// Writes the IL equivalent of a "for (int i = <paramref name="start"/>; condition(i); i += <paramref name="step"/>)" block<br/>
		/// This method is a specialized version of <see cref="EmitForLoop(ILCursor, Action{ILCursor}, Action{ILCursor, ILLabel}, Action{ILCursor}, Action{ILCursor})"/>
		/// </summary>
		/// <param name="c">The </param>
		/// <param name="start">The starting value for the loop variable</param>
		/// <param name="step">How much the loop variable should be incremented/decremented by per loop cycle</param>
		/// <param name="condition">The condition for the loop variable</param>
		/// <param name="body">The body of the loop</param>
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
