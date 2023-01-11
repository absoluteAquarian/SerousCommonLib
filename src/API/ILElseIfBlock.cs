using MonoMod.Cil;
using System;

namespace SerousCommonLib.API {
	/// <summary>
	/// A structure representing an "else if" block in IL
	/// </summary>
	public readonly struct ILElseIfBlock {
		/// <summary>
		/// The condition for the block
		/// </summary>
		public readonly Func<bool> condition;
		/// <summary>
		/// The instructions to write in the block's body
		/// </summary>
		public readonly Action<ILCursor> action;

		#pragma warning disable CS1591
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
