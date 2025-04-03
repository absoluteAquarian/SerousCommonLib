using System.Text;
using System;
using Terraria;

namespace SerousCommonLib.API.Input {
	/// <summary>
	/// An object representing the state for an <see cref="ITextInputActor"/>
	/// </summary>
	public class TextInputState {
		private int _cursor;
		private int _timer;
		private bool _focused;

		private bool _active, _oldActive;

		private string _initialText;
		private readonly StringBuilder _text;

		private readonly ITextInputActor _actor;

		/// <summary>
		/// Whether the text input actor has focus
		/// </summary>
		public bool HasFocus => _focused;

		/// <summary>
		/// Whether the text input actor has any text (if there is no text, the hint text is displayed)
		/// </summary>
		public bool HasText => _text.Length > 0;

		/// <summary>
		/// Whether the text input actor has any changes from the initial text
		/// </summary>
		public bool HasChanges => _text.ToString() != _initialText;

		/// <summary>
		/// Whether the text input actor is active
		/// </summary>
		public bool IsActive => _active;

		/// <summary>
		/// The current cursor location in the text input actor
		/// </summary>
		public int CursorLocation => _cursor;

		/// <summary>
		/// Whether the blinking cursor should be displayed
		/// </summary>
		public bool CursorBlink => _focused && _timer < 30;

		/// <summary>
		/// The currently inputted text in the text input actor
		/// </summary>
		public string InputText => _text.ToString();

		private string _asterisks;
		/// <summary>
		/// Whether the text input actor should hide its contents by replacing them with asterisks
		/// </summary>
		public bool HideContents { get; set; }

		/// <summary>
		/// Whether the text input actor should constantly have focus, useful for password prompts
		/// </summary>
		public bool ForcedFocus { get; set; }

		/// <summary>
		/// Whether the text input actor should lose focus when <see cref="ITextInputActor.OnInputEnter"/> is called.<br/>
		/// Defaults to <see langword="true"/>.
		/// </summary>
		public bool LoseFocusOnEnter { get; set; } = true;

		/// <summary>
		/// The <see cref="ITextInputActor"/> that this state is associated with
		/// </summary>
		public ITextInputActor Actor => _actor;

		internal TextInputState(ITextInputActor actor) {
			ArgumentNullException.ThrowIfNull(actor);
			_actor = actor;
			_text = new();
			_initialText = string.Empty;
		}

		/// <summary>
		/// Updates the state of the text input actor
		/// </summary>
		/// <param name="hoveringMouse">Whether the mouse is hovering over the actor</param>
		public void Tick(bool hoveringMouse) {
			if (_oldActive != _active) {
				if (_active)
					_actor.OnActivityGained();
				else
					_actor.OnActivityLost();

				_oldActive = _active;
			}

			if (++_timer >= 60)
				_timer = 0;

			if (ForcedFocus)
				Focus();
			else if (!hoveringMouse && (LegacyMouseInput.MouseClicked || LegacyMouseInput.RightMouseClicked))
				Unfocus();

			if (_active && _focused) {
				// Read from IME and update the text
				Main.blockInput = true;
				Main.drawingPlayerChat = false;
				Main.CurrentInputTextTakerOverride = this;

				int length = _text.Length;
				
				RawTextIME.Handle(_text, ref _cursor, _actor.Controller);

				if (_text.Length != length)
					_actor.OnInputChanged();

				if (Main.inputTextEnter) {
					_actor.OnInputEnter();
					if (LoseFocusOnEnter)
						Unfocus();
				} else if (Main.inputTextEscape)
					Reset(clearText: false);
			}

			_oldActive = _active;
		}

		/// <summary>
		/// Activates the text input actor
		/// </summary>
		public void Activate() => _active = true;

		/// <summary>
		/// Deactivates the text input actor
		/// </summary>
		public void Deactivate() => _active = false;

		/// <summary>
		/// Resets the text input actor, clearing the inputted text or reverting any changes
		/// </summary>
		/// <param name="clearText">Whether the text should be cleared entirely or any changes be reverted</param>
		public void Reset(bool clearText = true) {
			if (!clearText) {
				_text.Clear().Append(_initialText);
				_cursor = _text.Length;
				_actor.OnInputFocusLost();
			} else
				Clear();

			_timer = 0;
			_focused = false;
			TextInputTracker.CheckInputBlocking();
		}

		/// <summary>
		/// Clears the text input actor
		/// </summary>
		public void Clear() {
			_text.Clear();
			_initialText = string.Empty;
			_cursor = 0;
			_actor.OnInputCleared();
		}

		/// <summary>
		/// Sets the text input actor's input text and forces the cursor to the end of the text
		/// </summary>
		public void Set(string text) {
			_text.Clear().Append(text);
			_initialText = text;
			_cursor = _text.Length;
			_actor.OnInputChanged();
		}

		/// <summary>
		/// Attempts to gain focus for the text input actor
		/// </summary>
		public void Focus() {
			if (!_focused) {
				Main.blockInput = true;
				_focused = true;
				_cursor = _text.Length;
				_actor.OnInputFocusGained();
			}
		}

		/// <summary>
		/// Attempts to lose focus for the text input actor
		/// </summary>
		public void Unfocus() {
			if (!ForcedFocus && _focused) {
				_focused = false;
				_actor.OnInputFocusLost();
				TextInputTracker.CheckInputBlocking();
				_initialText = _text.ToString();
			}
		}

		/// <summary>
		/// Processes the currently inputted text for the text input actor and returns it.<br/>
		/// If no text is inputted, the hint text is returned instead.<br/>
		/// If the text input actor is set to hide its contents, the text is replaced with asterisks.
		/// </summary>
		public string GetCurrentText() {
			if (_text.Length <= 0)
				return _actor.HintText.Value;

			if (HideContents) {
				if (_asterisks == null || _asterisks.Length != _text.Length)
					_asterisks = new('*', _text.Length);

				return _asterisks;
			} else
				_asterisks = null;

			return _text.ToString();
		}
	}
}
