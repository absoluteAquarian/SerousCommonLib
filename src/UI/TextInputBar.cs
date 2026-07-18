using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using ReLogic.Graphics;
using ReLogic.Localization.IME;
using ReLogic.OS;
using SerousCommonLib.API.Input;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;

namespace SerousCommonLib.UI {
	/// <summary>
	/// A UI element that can receive and display text input
	/// </summary>
	public abstract class TextInputBar : UIElement, ITextInputActor {
		// Events so that making a derived class isn't necessary

		/// <inheritdoc cref="OnActivityGained"/>
		public event Action<TextInputBar> OnActivityGainedEvent;

		/// <inheritdoc cref="OnActivityLost"/>
		public event Action<TextInputBar> OnActivityLostEvent;

		/// <inheritdoc cref="OnInputChanged"/>
		public event Action<TextInputBar> OnInputChangedEvent;

		/// <inheritdoc cref="OnInputCleared"/>
		public event Action<TextInputBar> OnInputClearedEvent;

		/// <inheritdoc cref="OnInputEnter"/>
		public event Action<TextInputBar> OnInputEnterEvent;

		/// <inheritdoc cref="OnInputFocusGained"/>
		public event Action<TextInputBar> OnInputFocusGainedEvent;

		/// <inheritdoc cref="OnInputFocusLost"/>
		public event Action<TextInputBar> OnInputFocusLostEvent;

		/// <inheritdoc cref="RestrictedUpdate"/>
		public event Action<TextInputBar> OnRestrictedUpdate;

		/// <inheritdoc cref="PreStateTick"/>
		public event Action<TextInputBar> OnStateTick;

		/// <inheritdoc/>
		public TextInputState State { get; }

		/// <inheritdoc/>
		public LocalizedText HintText { get; set; }

		/// <inheritdoc/>
		public ITextInputController Controller { get; set; }

		/// <summary>
		/// Creates a new instance of <see cref="TextInputBar"/> with the provided hint text
		/// </summary>
		public TextInputBar(LocalizedText hintText) {
			_bar ??= SerousUtilities.Instance.Assets.Request<Texture2D>("Assets/SearchBar");
			_mouseFont ??= FontAssets.MouseText;

			// UISearchBar from Magic Storage uses a padding of 4 pixels, so that's kept here for consistency.
			// However, the only case it was used then was in a UIElement whose height was explicitly set to 32 pixels.
			// This height will be considered the default "minimum height" for the input bar.
			this.SetPadding(4);
			MinHeight.Set(32, 0f);

			State = TextInputTracker.ReserveState(this);

			HintText = hintText;
		}

		/// <inheritdoc/>
		public virtual void OnActivityGained() => OnActivityGainedEvent?.Invoke(this);

		/// <inheritdoc/>
		public virtual void OnActivityLost() => OnActivityLostEvent?.Invoke(this);

		/// <inheritdoc/>
		public virtual void OnInputChanged() => OnInputChangedEvent?.Invoke(this);

		/// <inheritdoc/>
		public virtual void OnInputCleared() => OnInputClearedEvent?.Invoke(this);

		/// <inheritdoc/>
		public virtual void OnInputEnter() => OnInputEnterEvent?.Invoke(this);

		/// <inheritdoc/>
		public virtual void OnInputFocusGained() => OnInputFocusGainedEvent?.Invoke(this);

		/// <inheritdoc/>
		public virtual void OnInputFocusLost() => OnInputFocusLostEvent?.Invoke(this);

		void ITextInputActor.Update(GameTime gameTime) => HandleState();

		/// <inheritdoc/>
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (!TextInputTracker.AreUpdatesPermitted() || !State.IsActive)
				RestrictedUpdate(gameTime);
		}

		private void HandleState() {
			PreStateTick();

			State.Tick(IsMouseHovering);
		}

		/// <summary>
		/// Executes whenever the text input actor is not active or the underlying state has yet to be updated with the latest text input, e.g. during <see cref="UIElement.Update"/>
		/// </summary>
		protected virtual void RestrictedUpdate(GameTime gameTime) => OnRestrictedUpdate?.Invoke(this);

		/// <summary>
		/// Executes just before the state for this text input actor is updated
		/// </summary>
		protected virtual void PreStateTick() => OnStateTick?.Invoke(this);

		/// <inheritdoc/>
		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);
			State.Focus();
		}
		
		/// <inheritdoc/>
		public override void RightClick(UIMouseEvent evt) {
			base.RightClick(evt);
			State.Reset(clearText: !State.HasFocus);
		}

		/// <inheritdoc/>
		public override void OnActivate() {
			base.OnActivate();

			State.Activate();
		}

		/// <inheritdoc/>
		public override void OnDeactivate() {
			base.OnDeactivate();

			State.Unfocus();
			State.Deactivate();
		}

		private const int Padding = 4;
		private static Asset<Texture2D> _bar;
		private static Asset<DynamicSpriteFont> _mouseFont;
		
		/// <inheritdoc/>
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			DrawBackBar(spriteBatch);
			DrawText(spriteBatch);
		}

		private void DrawBackBar(SpriteBatch spriteBatch) {
			Color color = Color.White;
			if (!PreDrawBackBar(spriteBatch, ref color))
				return;

			CalculatedStyle dim = GetDimensions();
			
			Texture2D texture = _bar.Value;
			
			Span<Rectangle> destinations = stackalloc Rectangle[9];
			Span<Rectangle> sources = stackalloc Rectangle[9];
			CalculateBarSlices(dim, destinations, sources);

			for (int i = 0; i < 9; i++)
				spriteBatch.Draw(texture, destinations[i], sources[i], color);
		}

		/// <summary>
		/// Executes before the bar for this text input actor is drawn
		/// </summary>
		/// <returns><see langword="true"/> to permit drawing the bar, <see langword="false"/> to prevent it.</returns>
		protected virtual bool PreDrawBackBar(SpriteBatch spriteBatch, ref Color color) => true;

		private void DrawText(SpriteBatch spriteBatch) {
			string text = State.GetCurrentText();
			Color textColor = Color.Black, hintColor = textColor * 0.75f;
			bool hasText = State.HasText;

			if (!PreDrawText(spriteBatch, ref textColor, ref hintColor))
				return;

			CalculatedStyle dim = GetDimensions();
			int innerHeight = (int)dim.Height - 2 * Padding;
			DynamicSpriteFont font = _mouseFont.Value;
			Vector2 size = font.MeasureString(text);
			float scale = innerHeight / size.Y;

			Color color = textColor;

			if (!hasText) {
				if (State.HasFocus) {
					// Remove the hint text
					text = string.Empty;
					hasText = true;
				} else {
					// Text is empty and we're not focused, so the hint text should be displayed
					color = hintColor;
				}
			}

			Vector2 origin = new Vector2(dim.X + Padding, dim.Y + Padding);
			bool focused = State.IsActive && State.HasFocus;

			// The IME composition string holds the characters being composed but not yet committed, e.g. the pinyin
			// typed so far for a Chinese IME. It is not part of the state's text until the IME commits it, so it has
			// to be drawn separately or the bar looks unresponsive while composing.
			string composition = focused ? Platform.Get<IImeService>().CompositionString : null;
			bool hasComposition = !string.IsNullOrEmpty(composition);

			float cursorOffset = hasText ? font.MeasureString(text[..State.CursorLocation]).X * scale : 0f;

			if (!hasComposition)
			{
				spriteBatch.DrawString(font, text, origin, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
			else
			{
				// Split the text so that the composition is shown at the cursor rather than always at the end
				float compositionWidth = font.MeasureString(composition).X * scale;

				spriteBatch.DrawString(font, text[..State.CursorLocation], origin, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
				spriteBatch.DrawString(font, composition, origin + new Vector2(cursorOffset, 0f), CompositionColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
				spriteBatch.DrawString(font, text[State.CursorLocation..], origin + new Vector2(cursorOffset + compositionWidth, 0f), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

				cursorOffset += compositionWidth;
			}

			if (hasText && State.IsActive && State.CursorBlink)
			{
				spriteBatch.DrawString(font, "|", origin + new Vector2(cursorOffset, 0f), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}

			// Terraria hides the OS candidate window and renders the list itself, but only where DrawWindowsIMEPanel is
			// called. Without this the IME still works and commits characters, but the candidates are invisible.
			// The panel draws a strip upwards from the anchor, so anchor past the cursor like vanilla's chat does.
			if (focused)
			{
				Main.instance.DrawWindowsIMEPanel(origin + new Vector2(cursorOffset + 10f, -6f));
			}
		}

		/// <summary>
		/// The color used to draw the in-progress IME composition string.  Defaults to the same yellow used by vanilla's chat.
		/// </summary>
		protected virtual Color CompositionColor => new Color(255, 240, 20);

		/// <summary>
		/// Executes before the text for this text input actor is drawn
		/// </summary>
		/// <returns><see langword="true"/> to permit drawing the text, <see langword="false"/> to prevent it.</returns>
		protected virtual bool PreDrawText(SpriteBatch spriteBatch, ref Color textColor, ref Color hintColor) => true;

		private static void CalculateBarSlices(CalculatedStyle dimensions, Span<Rectangle> destinations, Span<Rectangle> sources) {
			int x = (int)dimensions.X;
			int y = (int)dimensions.Y;
			int innerWidth = (int)dimensions.Width - 2 * Padding;
			int innerHeight = (int)dimensions.Height - 2 * Padding;

			destinations[0] = new Rectangle(x,                        y,                         Padding,    Padding);
			destinations[1] = new Rectangle(x + Padding,              y,                         innerWidth, Padding);
			destinations[2] = new Rectangle(x + Padding + innerWidth, y,                         Padding,    Padding);
			destinations[3] = new Rectangle(x,                        y + Padding,               Padding,    innerHeight);
			destinations[4] = new Rectangle(x + Padding,              y + Padding,               innerWidth, innerHeight);
			destinations[5] = new Rectangle(x + Padding + innerWidth, y + Padding,               Padding,    innerHeight);
			destinations[6] = new Rectangle(x,                        y + Padding + innerHeight, Padding,    Padding);
			destinations[7] = new Rectangle(x + Padding,              y + Padding + innerHeight, innerWidth, Padding);
			destinations[8] = new Rectangle(x + Padding + innerWidth, y + Padding + innerHeight, Padding,    Padding);

			sources[0] = new Rectangle(0,           0,           Padding, Padding);
			sources[1] = new Rectangle(Padding,     0,           1,       Padding);
			sources[2] = new Rectangle(Padding + 1, 0,           Padding, Padding);
			sources[3] = new Rectangle(0,           Padding,     Padding, 1);
			sources[4] = new Rectangle(Padding,     Padding,     1,       1);
			sources[5] = new Rectangle(Padding + 1, Padding,     Padding, 1);
			sources[6] = new Rectangle(0,           Padding + 1, Padding, Padding);
			sources[7] = new Rectangle(Padding,     Padding + 1, 1,       Padding);
			sources[8] = new Rectangle(Padding + 1, Padding + 1, Padding, Padding);
		}
	}
}
