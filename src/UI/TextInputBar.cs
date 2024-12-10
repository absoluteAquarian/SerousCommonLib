using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using ReLogic.Graphics;
using SerousCommonLib.API.Input;
using System;
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

			spriteBatch.DrawString(font, text, new Vector2(dim.X + Padding, dim.Y + Padding), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

			if (hasText && State.CursorBlink) {
				float drawCursor = font.MeasureString(text[..State.CursorLocation]).X * scale;
				spriteBatch.DrawString(font, "|", new Vector2(dim.X + Padding + drawCursor, dim.Y + Padding), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
		}

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
