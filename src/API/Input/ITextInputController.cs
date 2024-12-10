namespace SerousCommonLib.API.Input {
	/// <summary>
	/// Represents an object that can monitor and control text input given to a <see cref="ITextInputActor"/>
	/// </summary>
	public interface ITextInputController {
		/// <summary>
		/// Return <see langword="false"/> to prevent the character from being used as input, or <see langword="true"/> otherwise.
		/// </summary>
		bool PermitCharacter(char c);
	}
}
