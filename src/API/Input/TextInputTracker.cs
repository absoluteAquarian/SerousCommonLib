using Microsoft.Xna.Framework;
using SerousCommonLib.UI;
using System.Collections.Generic;
using Terraria;

namespace SerousCommonLib.API.Input {
	/// <summary>
	/// A helper class that manages text input for <see cref="ITextInputActor"/> instances
	/// </summary>
	public static class TextInputTracker {
		private static readonly List<TextInputState> _inputs = new();

		private static bool fullInputBarUpdating;

		internal static void Unload() {
			_inputs.Clear();
			Main.blockInput = false;
		}

		/// <summary>
		/// Permits full updates for all <see cref="TextInputBar"/> instances
		/// </summary>
		public static void RequestFullUpdates() => fullInputBarUpdating = true;

		/// <summary>
		/// Restricts full updates for all <see cref="TextInputBar"/> instances
		/// </summary>
		public static void RestrictUpdates() => fullInputBarUpdating = false;

		/// <summary>
		/// Returns whether full updates for <see cref="TextInputBar"/> instances are restricted
		/// </summary>
		/// <returns></returns>
		public static bool AreUpdatesPermitted() => fullInputBarUpdating;

		/// <summary>
		/// Assigns a state to the provided <see cref="ITextInputActor"/> and registers it for input tracking
		/// </summary>
		/// <param name="actor">The actor being tracked</param>
		/// <returns>The state of the actor</returns>
		public static TextInputState ReserveState(ITextInputActor actor) {
			var state = new TextInputState(actor);
			_inputs.Add(state);
			return state;
		}

		/// <summary>
		/// Updates <see cref="Main.blockInput"/> and <see cref="Main.CurrentInputTextTakerOverride"/> depending on whether any <see cref="TextInputState"/> is active and has focus.<br/>
		/// This method also forcibly closes the ingame chat when called.
		/// </summary>
		public static void CheckInputBlocking() {
			if (_inputs.Find(static s => s.IsActive && s.HasFocus) is { } input) {
				Main.CurrentInputTextTakerOverride = input;
				Main.blockInput = true;
			} else
				Main.blockInput = false;
			
			// Always block the chat when calling this method
			Main.drawingPlayerChat = false;
			Main.chatRelease = false;
		}

		/// <summary>
		/// Returns the currently focused <see cref="TextInputState"/>
		/// </summary>
		public static TextInputState GetFocusedState() => _inputs.Find(static s => s.HasFocus);

		internal static void Update(GameTime gameTime) {
			foreach (var input in _inputs) {
				if (input.IsActive)
					input.Actor.Update(gameTime);
			}
		}
	}
}
