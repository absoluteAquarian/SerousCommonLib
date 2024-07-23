using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace SerousCommonLib.UI {
	/// <summary>
	/// A modified copy of <see cref="UIScrollbar"/>.  For horizontal scrolling, use <see cref="NewUIScrollBarHorizontal"/>
	/// </summary>
	public class NewUIScrollbar : UIElement {
		#pragma warning disable CS1591
		private float _viewPosition;

		public float ViewSize { get; private set; } = 1f;
		public float MaxViewSize { get; private set; } = 20f;
		
		public bool IsDragging { get; private set; }
		
		private bool _isHoveringOverHandle;
		private float _dragOffset;
		private Asset<Texture2D> _texture;
		private Asset<Texture2D> _innerTexture;

		public event Action<NewUIScrollbar> OnDraggingStart, OnDraggingEnd;

		// TODO: should this even be a member?  figure out why Magic Storage uses it
		public bool IgnoreParentBoundsWhenDrawing { get; set; }

		public float ScrollDividend { get; set; } = 1f;

		public float ViewPosition {
			get {
				return _viewPosition;
			}
			set {
				_viewPosition = MathHelper.Clamp(value, 0f, MaxViewSize - ViewSize);
			}
		}

		public bool CanScroll => MaxViewSize != ViewSize;

		public void GoToBottom() {
			ViewPosition = MaxViewSize - ViewSize;
		}

		public NewUIScrollbar(float scrollDividend = 1f) {
			if (VerticalScrolling) {
				Width.Set(20f, 0f);
				MaxWidth.Set(20f, 0f);
				PaddingTop = 5f;
				PaddingBottom = 5f;
			} else {
				Height.Set(20f, 0f);
				MaxHeight.Set(20f, 0f);
				PaddingLeft = 5f;
				PaddingRight = 5f;
			}

			ScrollDividend = scrollDividend;
		}

		public virtual bool VerticalScrolling => true;

		public void SetView(float viewSize, float maxViewSize) {
			viewSize = MathHelper.Clamp(viewSize, 0f, maxViewSize);
			_viewPosition = MathHelper.Clamp(_viewPosition, 0f, maxViewSize - viewSize);
			ViewSize = viewSize;
			MaxViewSize = maxViewSize;
		}

		public Rectangle GetHandleRectangle() => GetHandleRectangle(GetInnerDimensions());

		private Rectangle GetHandleRectangle(CalculatedStyle style) {
			if (MaxViewSize == 0f && ViewSize == 0f) {
				ViewSize = 1f;
				MaxViewSize = 1f;
			}

			if (VerticalScrolling)
				return new Rectangle((int)style.X, (int)(style.Y + style.Height * (_viewPosition / MaxViewSize)), 20, (int)(style.Height * (ViewSize / MaxViewSize)) + 7);
			else
				return new Rectangle((int)(style.X + style.Width * (_viewPosition / MaxViewSize)), (int)style.Y, (int)(style.Width * (ViewSize / MaxViewSize)) + 7, 20);
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			CalculatedStyle innerDimensions = GetInnerDimensions();
			float size = VerticalScrolling ? innerDimensions.Height : innerDimensions.Width;
			if (IsDragging) {
				float num = (VerticalScrolling ? Main.MouseScreen.Y - innerDimensions.Y : Main.MouseScreen.X - innerDimensions.X) - _dragOffset;
				_viewPosition = MathHelper.Clamp(num / size * MaxViewSize, 0f, MaxViewSize - ViewSize);
			}

			if (size > 0) {
				Rectangle handleRectangle = GetHandleRectangle();
				Vector2 mousePosition = Main.MouseScreen;
				bool isHoveringOverHandle = _isHoveringOverHandle;
				_isHoveringOverHandle = handleRectangle.Contains(new Point((int)mousePosition.X, (int)mousePosition.Y));
				if (!isHoveringOverHandle && _isHoveringOverHandle && Main.hasFocus && !IsDragging)
					SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		public override void Recalculate() {
			StyleDimension copyMinWidth = MinWidth;
			StyleDimension copyMaxWidth = MaxWidth;
			StyleDimension copyMinHeight = MinHeight;
			StyleDimension copyMaxHeight = MaxHeight;

			if (IgnoreParentBoundsWhenDrawing) {
				MinWidth = StyleDimension.Fill;
				MaxWidth = StyleDimension.Fill;
				MinHeight = StyleDimension.Fill;
				MaxHeight = StyleDimension.Fill;
			}

			base.Recalculate();

			if (IgnoreParentBoundsWhenDrawing) {
				MinWidth = copyMinWidth;
				MaxWidth = copyMaxWidth;
				MinHeight = copyMinHeight;
				MaxHeight = copyMaxHeight;
			}
		}

		private void DrawBar(SpriteBatch spriteBatch, Texture2D texture, Rectangle dimensions, Color color) {
			if (VerticalScrolling) {
				spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y - 6, dimensions.Width, 6), new Rectangle(0, 0, texture.Width, 6), color);
				spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height), new Rectangle(0, 6, texture.Width, 4), color);
				spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y + dimensions.Height, dimensions.Width, 6), new Rectangle(0, texture.Height - 6, texture.Width, 6), color);
			} else {
				spriteBatch.Draw(texture, new Rectangle(dimensions.X - 6, dimensions.Y, 6, dimensions.Height), new Rectangle(0, 0, 6, texture.Height), color);
				spriteBatch.Draw(texture, new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height), new Rectangle(6, 0, 4, texture.Height), color);
				spriteBatch.Draw(texture, new Rectangle(dimensions.X + dimensions.Width, dimensions.Y, 6, dimensions.Height), new Rectangle(texture.Width - 6, 0, 6, texture.Height), color);
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			_texture ??= VerticalScrolling
				? Main.Assets.Request<Texture2D>("Images/UI/Scrollbar")
				: ModContent.Request<Texture2D>("SerousCommonLib/Assets/ScrollbarHorizontal");

			_innerTexture ??= VerticalScrolling
				? Main.Assets.Request<Texture2D>("Images/UI/ScrollbarInner")
				: ModContent.Request<Texture2D>("SerousCommonLib/Assets/ScrollbarInnerHorizontal");

			CalculatedStyle dimensions;
			Rectangle handleRectangle;

			if (!IgnoreParentBoundsWhenDrawing) {
				dimensions = GetDimensions();
				handleRectangle = GetHandleRectangle();
			} else {
				GetUnrestrictedDimensions(out _, out dimensions, out CalculatedStyle innerDimensions);
				handleRectangle = GetHandleRectangle(innerDimensions);
			}

			DrawBar(spriteBatch, _texture.Value, dimensions.ToRectangle(), Color.White);
			DrawBar(spriteBatch, _innerTexture.Value, handleRectangle, Color.White * ((IsDragging || _isHoveringOverHandle) ? 1f : 0.85f));
		}

		// Copy of UIElement.Recalculate() and UIElement.GetDimensionsBasedOnParentDimensions(), but without any clamping for Width and Height
		private void GetUnrestrictedDimensions(out CalculatedStyle outer, out CalculatedStyle style, out CalculatedStyle inner) {
			CalculatedStyle parentDimensions = Parent is null ? new CalculatedStyle(0, 0, Main.screenWidth, Main.screenHeight) : Parent.GetInnerDimensions();

			CalculatedStyle result = default;
			result.X = Left.GetValue(parentDimensions.Width) + parentDimensions.X;
			result.Y = Top.GetValue(parentDimensions.Height) + parentDimensions.Y;
			//float value = MinWidth.GetValue(parentDimensions.Width);
			//float value2 = MaxWidth.GetValue(parentDimensions.Width);
			//float value3 = MinHeight.GetValue(parentDimensions.Height);
			//float value4 = MaxHeight.GetValue(parentDimensions.Height);
			//result.Width = MathHelper.Clamp(Width.GetValue(parentDimensions.Width), value, value2);
			//result.Height = MathHelper.Clamp(Height.GetValue(parentDimensions.Height), value3, value4);
			result.Width = Width.GetValue(parentDimensions.Width);
			result.Height = Height.GetValue(parentDimensions.Height);
			result.Width += MarginLeft + MarginRight;
			result.Height += MarginTop + MarginBottom;
			result.X += parentDimensions.Width * HAlign - result.Width * HAlign;
			result.Y += parentDimensions.Height * VAlign - result.Height * VAlign;
			outer = result;

			result.X += MarginLeft;
			result.Y += MarginTop;
			result.Width -= MarginLeft + MarginRight;
			result.Height -= MarginTop + MarginBottom;
			style = result;

			result.X += PaddingLeft;
			result.Y += PaddingTop;
			result.Width -= PaddingLeft + PaddingRight;
			result.Height -= PaddingTop + PaddingBottom;
			inner = result;
		}

#if TML_2022_09
		public override void MouseDown(UIMouseEvent evt) {
			base.MouseDown(evt);
#else
		public override void LeftMouseDown(UIMouseEvent evt) {
			base.LeftMouseDown(evt);
#endif
			if (evt.Target == this) {
				Rectangle handleRectangle = GetHandleRectangle();
				if (handleRectangle.Contains(new Point((int)evt.MousePosition.X, (int)evt.MousePosition.Y))) {
					if (!IsDragging)
						OnDraggingStart?.Invoke(this);

					IsDragging = true;
					_dragOffset = VerticalScrolling ? evt.MousePosition.Y - handleRectangle.Y : evt.MousePosition.X - handleRectangle.X;
				} else {
					CalculatedStyle innerDimensions = GetInnerDimensions();
					float num = VerticalScrolling 
						? Main.MouseScreen.Y - innerDimensions.Y - handleRectangle.Height / 2
						: Main.MouseScreen.X - innerDimensions.X - handleRectangle.Width / 2;
					_viewPosition = MathHelper.Clamp(num / (VerticalScrolling ? innerDimensions.Height : innerDimensions.Width) * MaxViewSize, 0f, MaxViewSize - ViewSize);
				}
			}
		}

#if TML_2022_09
		public override void MouseUp(UIMouseEvent evt) {
			base.MouseUp(evt);
#else
		public override void LeftMouseUp(UIMouseEvent evt) {
			base.LeftMouseUp(evt);
#endif

			if (IsDragging)
				OnDraggingEnd?.Invoke(this);

			IsDragging = false;
		}

		public override void ScrollWheel(UIScrollWheelEvent evt) {
			if (IsDragging)
				return;

			base.ScrollWheel(evt);

			_viewPosition -= evt.ScrollWheelValue / ScrollDividend;
		}
	}
}
