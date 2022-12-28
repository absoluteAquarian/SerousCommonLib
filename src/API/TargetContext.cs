using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Linq;

namespace SerousCommonLib.API {
	public readonly struct TargetContext {
		public readonly ILCursor cursor;
		public readonly int targetIndex;
		public readonly IReadOnlyList<ILLabel> incomingLabels;
		public readonly IReadOnlyList<ExceptionHandler> incomingHandlers;

		public TargetContext(ILCursor c) {
			cursor = c;
			incomingLabels = c.IncomingLabels.ToList().AsReadOnly();
			incomingHandlers = c.IncomingHandlers().ToList().AsReadOnly();

			targetIndex = c.Index;
		}

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
