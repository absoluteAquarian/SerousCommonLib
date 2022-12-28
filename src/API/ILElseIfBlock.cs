using MonoMod.Cil;
using System;

namespace SerousCommonLib.API {
	public readonly struct ILElseIfBlock {
		public readonly Func<bool> condition;
		public readonly Action<ILCursor> action;

		public ILElseIfBlock(Func<bool> condition, Action<ILCursor> action) {
			this.condition = condition;
			this.action = action;
		}

		public void Deconstruct(out Func<bool> condition, out Action<ILCursor> action) {
			condition = this.condition;
			action = this.action;
		}
	}
}
