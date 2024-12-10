using System;

namespace SerousCommonLib.API.Input {
	/// <summary>
	/// A basic implementation of a <see cref="ITextInputController"/> that restricts certain characters from being input
	/// </summary>
	public class SimpleTextInputController : ITextInputController {
		private readonly char[] _restrictedCharacters;

		/// <summary>
		/// Creates a new instance of <see cref="SimpleTextInputController"/> with the provided restricted characters
		/// </summary>
		public SimpleTextInputController(params char[] restrictedCharacters) {
			ArgumentNullException.ThrowIfNull(restrictedCharacters);

			_restrictedCharacters = restrictedCharacters;
		}

		bool ITextInputController.PermitCharacter(char c) => Array.IndexOf(_restrictedCharacters, c) == -1;
	}
}
