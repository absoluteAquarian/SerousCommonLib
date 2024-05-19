using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Linq;

namespace SerousCommonLib.API {
	/// <summary>
	/// A helper structure for quickly modifying branching labels
	/// </summary>
	public readonly struct TargetContext {
		/// <summary>
		/// The cursor performing edits
		/// </summary>
		public readonly ILCursor cursor;
		/// <summary>
		/// The index of the instruction to redirect <see cref="incomingLabels"/> and <see cref="incomingHandlers"/> to
		/// </summary>
		public readonly int targetIndex;
		/// <summary>
		/// A list of branching labels
		/// </summary>
		public readonly IReadOnlyList<ILLabel> incomingLabels;
		/// <summary>
		/// A list of branching labels for try-catch-finally clauses
		/// </summary>
		public readonly IReadOnlyList<ExceptionHandler> incomingHandlers;

		/// <summary>
		/// Creates a new <see cref="TargetContext"/> from the provided <see cref="ILCursor"/>.  This context records the current cursor's incoming labels and exception handlers for later modification.
		/// </summary>
		/// <param name="c"></param>
		public TargetContext(ILCursor c) {
			cursor = c;
			incomingLabels = c.IncomingLabels.ToList().AsReadOnly();
			incomingHandlers = c.IncomingHandlers().ToList().AsReadOnly();

			targetIndex = c.Index;
		}

		/// <summary>
		/// Updates the instructions in <see cref="cursor"/> whose target's instruction index has been modified
		/// </summary>
		public void UpdateInstructions() {
			Instruction target = cursor.Instrs[targetIndex];
				
			// Retarget the labels to the new target instruction
			foreach (var label in incomingLabels)
				label.Target = target;
				
			// Retarget the exception handlers to the new target instruction
			foreach (var handler in incomingHandlers)
				handler.HandlerEnd = target;
		}
	}
}
