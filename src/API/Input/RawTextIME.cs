using Microsoft.Xna.Framework.Input;
using ReLogic.Localization.IME;
using ReLogic.OS;
using System.Collections.Generic;
using System.Text;
using System;
using Terraria.GameInput;
using Terraria;
using Terraria.ModLoader;

namespace SerousCommonLib.API.Input {
	/// <summary>
	/// A helper class that reimplements Terraria's input system to improve performance, allow insertion of characters in the middle of text and remove checks for chat tag usage.<br/>
	/// Additionally, it also comes with more controls: <br/>
	/// <list type="bullet">
	/// <item>Left arrow - move the cursor to the left by one character</item>
	/// <item>Right arrow - move the cursor to the right by one character</item>
	/// <item>Home - move the cursor to before the first character of the text</item>
	/// <item>End - move the cursor to after the last character in the text</item>
	/// <item>
	/// Ctrl+Backspace - delete one "word" in the text, where a word is any text following a whitespace.<br/>
	/// If there is no whitespace to the left of the word, everything to the left of the cursor is deleted.
	/// </item>
	/// </list>
	/// </summary>
	public class RawTextIME : ModSystem {
		private static bool _hasIMEListener;

		/// <inheritdoc/>
		public override void Load() {
			if (!Main.dedServ) {
				Platform.Get<IImeService>().AddKeyListener(listener: Listen);
				_hasIMEListener = true;
			}
		}
		
		/// <inheritdoc/>
		public override void Unload() {
			if (_hasIMEListener) {
				Platform.Get<IImeService>().RemoveKeyListener(listener: Listen);
				_hasIMEListener = false;
			}
		}

		private static readonly Queue<char> _inputQueue = new();

		private static int backSpaceCount;
		private static float backSpaceRate;

		private static void Listen(char key) {
			// Force vanilla to be cleared when this system is active, and vice versa
			if (TextInputTracker.GetFocusedState() is not null) {
				Main.keyCount = 0;

				_inputQueue.Enqueue(key);

				// Limit the queue to 10 characters, destroying the oldest ones
				if (_inputQueue.Count > 10)
					_inputQueue.Dequeue();
			} else {
				_inputQueue.Clear();
			}
		}

		/// <summary>
		/// Handles text input for adding text to the end of a <see cref="StringBuilder"/> instance, optionally checking for chat tags.
		/// </summary>
		/// <param name="text">The text builder</param>
		/// <param name="controller">An optional object used to control the input passed to <paramref name="text"/></param>
		public static void Handle(StringBuilder text, ITextInputController controller = null) {
			int cursor = text.Length;
			Handle(text, ref cursor);
		}
		
		/// <summary>
		/// The virtual key identifier for the Tab key
		/// </summary>
		public const int VK_TAB = 0x09;
		/// <summary>
		/// The virtual key identifier for the Enter key
		/// </summary>
		public const int VK_ENTER = 0x0D;
		/// <summary>
		/// The virtual key identifier for the Escape key
		/// </summary>
		public const int VK_ESCAPE = 0x1B;

		/// <summary>
		/// Handles text input for inserting text into a <see cref="StringBuilder"/> instance, optionally checking for chat tags.
		/// </summary>
		/// <param name="text">The text builder</param>
		/// <param name="cursor">A reference to the cursor location to insert characters at</param>
		/// <param name="controller">An optional object used to control the input passed to <paramref name="text"/></param>
		public static void Handle(StringBuilder text, ref int cursor, ITextInputController controller = null) {
			// These two lines are important for enabling the key listener event above
			PlayerInput.WritingText = true;
			Main.instance.HandleIME();

			if (!Main.hasFocus)
				return;

			Main.inputTextEnter = false;
			Main.inputTextEscape = false;
			
			Main.oldInputText = Main.inputText;
			Main.inputText = Keyboard.GetState();

			// Handle control sequences for clearing, copying, cutting and pasting
			if (Main.inputText.PressingControl()) {
				if (KeyTyped(Keys.Z)) {
					// Clear the text
					text.Length = 0;
					cursor = 0;
					goto cleanup;
				} else if (KeyTyped(Keys.X)) {
					// Cut: Ctrl+X
					Platform.Get<IClipboard>().Value = text.ToString();
					text.Length = 0;
					cursor = 0;
					goto cleanup;
				} else if (KeyTyped(Keys.C) || KeyTyped(Keys.Insert)) {
					// Copy: Ctrl+C or Ctrl+Insert
					Platform.Get<IClipboard>().Value = text.ToString();
					goto cleanup;
				} else if (KeyTyped(Keys.V)) {
					// Paste: Ctrl+V
					string paste = Platform.Get<IClipboard>().Value;
					text.Insert(cursor, paste);
					cursor += paste.Length;
					goto cleanup;
				} else if (KeyTyped(Keys.Back)) {
					// Delete the last "word" in the text
					ReadOnlySpan<char> currentText = text.ToString();
					
					int i;
					// Look for the first non-whitespace character
					for (i = cursor; i >= 0; i--) {
						if (!char.IsWhiteSpace(currentText[i]))
							break;
					}
					// Look for the first whitespace character
					for (; i >= 0; i--) {
						if (char.IsWhiteSpace(currentText[i]))
							break;
					}

					if (i < 0) {
						// Delete everything starting at the cursor
						text.Remove(0, cursor);
						cursor = 0;
					} else {
						// Delete everthing between the found whitespace and the cursor
						int nonWS = i + 1;
						text.Remove(nonWS, cursor - nonWS);
						cursor = nonWS;
					}

					goto cleanup;
				}
			} else if (Main.inputText.PressingShift()) {
				if (KeyTyped(Keys.Delete)) {
					// Cut: Shift+Delete
					Platform.Get<IClipboard>().Value = text.ToString();
					text.Length = 0;
					cursor = 0;
					goto cleanup;
				} else if (KeyTyped(Keys.Insert)) {
					// Paste: Shift+Insert
					string paste = Platform.Get<IClipboard>().Value;
					text.Insert(cursor, paste);
					cursor += paste.Length;
					goto cleanup;
				}
			}

			// Handle character input, single character deletion via Backspace and cursor manipulation
			bool canDeleteAKey = KeyTyped(Keys.Back);

			if (KeyHeld(Keys.Back)) {
				backSpaceRate -= 0.05f;
				if (backSpaceRate < 0f)
					backSpaceRate = 0f;

				if (backSpaceCount <= 0) {
					backSpaceCount = (int)Math.Round(backSpaceRate);
					canDeleteAKey = true;
				}

				backSpaceCount--;
			} else {
				backSpaceRate = 7f;
				backSpaceCount = 15;
			}

			if (KeyTyped(Keys.Left) && cursor > 0) {
				cursor--;
				goto cleanup;
			} else if (KeyTyped(Keys.Right)) {
				cursor++;
				goto cleanup;
			} else if (KeyTyped(Keys.Home)) {
				cursor = 0;
				goto cleanup;
			} else if (KeyTyped(Keys.End)) {
				cursor = text.Length;
				goto cleanup;
			} else if (canDeleteAKey && cursor > 0) {
				text.Remove(--cursor, 1);
				goto cleanup;
			}

			// Process the character queue
			while (_inputQueue.TryDequeue(out char c)) {
				if (controller?.PermitCharacter(c) is false)
					continue;

				if (c == VK_ENTER)
					Main.inputTextEnter = true;
				else if (c == VK_ESCAPE || c == VK_TAB)
					Main.inputTextEscape = true;
				else if (c >= 32 && c != 127)
					text.Insert(cursor++, c);
			}

			cleanup:
			// Always empty the queue after executing this handler
			_inputQueue.Clear();
		}

		private static bool KeyHeld(Keys key) => Main.inputText.IsKeyDown(key) && Main.oldInputText.IsKeyDown(key);

		private static bool KeyTyped(Keys key) => Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
	}
}
