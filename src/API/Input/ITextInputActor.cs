using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace SerousCommonLib.API.Input {
	/// <summary>
	/// Represents an actor that can receive text input
	/// </summary>
	public interface ITextInputActor {
		/// <summary>
		/// The current state of the text input actor
		/// </summary>
		TextInputState State { get; }

		/// <summary>
		/// The text to display when the text input actor is empty
		/// </summary>
		LocalizedText HintText { get; }

		/// <summary>
		/// This method executes whenever the text input actor is active
		/// </summary>
		void Update(GameTime gameTime);

		/// <summary>
		/// This method executes whenever the text input actor is activated
		/// </summary>
		void OnActivityGained();

		/// <summary>
		/// This method executes whenever the text input actor is deactivated
		/// </summary>
		void OnActivityLost();

		/// <summary>
		/// This method executes when the text input actor's text has changed
		/// </summary>
		void OnInputChanged();

		/// <summary>
		/// This method executes whenever the text input actor's text is cleared
		/// </summary>
		void OnInputCleared();

		/// <summary>
		/// This method executes when the text input actor's text is submitted (e.g. by pressing Enter)
		/// </summary>
		void OnInputEnter();

		/// <summary>
		/// This method executes when the text input actor gains focus and starts blocking gameplay input
		/// </summary>
		void OnInputFocusGained();

		/// <summary>
		/// This method executes when the text input actor loses focus and stops blocking gameplay input
		/// </summary>
		void OnInputFocusLost();
	}
}
