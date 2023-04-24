using MonoMod.Cil;
using System;

namespace SerousCommonLib.API {
	/// <summary>
	/// A structure representing an "else if" block in IL
	/// </summary>
	public readonly struct ILElseIfBlock {
		/// <summary>
		/// An optional function that's invoked before emitting the delegate for <see cref="condition"/>
		/// </summary>
		public readonly Action<ILCursor> preConditionAction;
		/// <summary>
		/// The condition for the block
		/// </summary>
		public readonly Func<bool> condition;
		/// <summary>
		/// If not null, this action acts as the delegate used for emitting the condition
		/// </summary>
		public readonly Action<ILCursor> conditionDirect;
		/// <summary>
		/// The instructions to write in the block's body
		/// </summary>
		public readonly Action<ILCursor> action;

		#pragma warning disable CS1591
		public ILElseIfBlock(Func<bool> condition, Action<ILCursor> action) {
			this.condition = condition;
			this.action = action;
		}

		public ILElseIfBlock(Action<ILCursor> preConditionAction, Func<bool> condition, Action<ILCursor> action) {
			this.preConditionAction = preConditionAction;
			this.condition = condition;
			this.action = action;
		}

		public ILElseIfBlock(Action<ILCursor> conditionDirect, Action<ILCursor> action) {
			this.conditionDirect = conditionDirect;
			this.action = action;
		}

		public void Deconstruct(out Action<ILCursor> preConditionAction, out Func<bool> condition, out Action<ILCursor> conditionDirect, out Action<ILCursor> action) {
			preConditionAction = this.preConditionAction;
			condition = this.condition;
			conditionDirect = this.conditionDirect;
			action = this.action;
		}
	}
}
