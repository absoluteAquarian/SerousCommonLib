using System;

namespace SerousCommonLib.API.Input {
	/// <summary>
	/// A basic implementation of a <see cref="ITextInputController"/> that restricts certain characters from being input based on a function
	/// </summary>
	public class LambdaTextInputController : ITextInputController {
		private readonly Func<char, bool> _permitCharacter;

		/// <summary>
		/// Creates a new instance of <see cref="LambdaTextInputController"/> with the provided function
		/// </summary>
		/// <param name="permitCharacter"></param>
		public LambdaTextInputController(Func<char, bool> permitCharacter) {
			ArgumentNullException.ThrowIfNull(permitCharacter);

			_permitCharacter = permitCharacter;
		}

		bool ITextInputController.PermitCharacter(char c) => _permitCharacter(c);
	}
}
