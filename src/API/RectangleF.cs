using Microsoft.Xna.Framework;
using System;

namespace SerousCommonLib.API {
	/// <summary>
	/// A structure representing a rectangle with floating point precision
	/// </summary>
	public struct RectangleF {
        /// <summary>
		/// The x-coordinate of the rectangle
		/// </summary>
		public float X;
		
		/// <summary>
		/// The y-coordinate of the rectangle
		/// </summary>
		public float Y;
		
		/// <summary>
		/// The width of the rectangle
		/// </summary>
		public float Width;
		
		/// <summary>
		/// The height of the rectangle
		/// </summary>
		public float Height;

		/// <summary>
		/// Creates a new rectangle with the specified position and size
		/// </summary>
		/// <param name="x">The x-coordinate of the rectangle</param>
		/// <param name="y">The y-coordinate of the rectangle</param>
		/// <param name="width">The width of the rectangle</param>
		/// <param name="height">The height of the rectangle</param>
		public RectangleF(float x, float y, float width, float height) {
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Creates a new rectangle with the specified position and size
		/// </summary>
		/// <param name="position">The position of the rectangle</param>
		/// <param name="size">The size of the rectangle</param>
		public RectangleF(Vector2 position, Vector2 size) {
			X = position.X;
			Y = position.Y;
			Width = size.X;
			Height = size.Y;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the top-left corner
		/// </summary>
		public void InflateFromTopLeft(float horizontalAmount, float verticalAmount) {
			Width += horizontalAmount;
			Height += verticalAmount;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the center of the top edge
		/// </summary>
		public void InflateFromTopCenter(float horizontalAmount, float verticalAmount) {
			X -= horizontalAmount / 2;
			Width += horizontalAmount;
			Height += verticalAmount;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the top-right corner
		/// </summary>
		public void InflateFromTopRight(float horizontalAmount, float verticalAmount) {
			X -= horizontalAmount;
			Width += horizontalAmount;
			Height += verticalAmount;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the center of the left edge
		/// </summary>
		public void InflateFromCenterLeft(float horizontalAmount, float verticalAmount) {
			Y -= verticalAmount / 2;
			Width += horizontalAmount;
			Height += verticalAmount;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, centered around the rectangle's center
		/// </summary>
		public void InflateFromCenter(float horizontalAmount, float verticalAmount) {
			X -= horizontalAmount / 2;
			Y -= verticalAmount / 2;
			Width += horizontalAmount;
			Height += verticalAmount;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the center of the right edge
		/// </summary>
		public void InflateFromCenterRight(float horizontalAmount, float verticalAmount) {
			X -= horizontalAmount;
			Y -= verticalAmount / 2;
			Width += horizontalAmount;
			Height += verticalAmount;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the bottom-left corner
		/// </summary>
		public void InflateFromBottomLeft(float horizontalAmount, float verticalAmount) {
			Y -= verticalAmount;
			Width += horizontalAmount;
			Height += verticalAmount;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the center of the bottom edge
		/// </summary>
		public void InflateFromBottomCenter(float horizontalAmount, float verticalAmount) {
			X -= horizontalAmount / 2;
			Y -= verticalAmount;
			Width += horizontalAmount;
			Height += verticalAmount;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the bottom-right corner
		/// </summary>
		public void InflateFromBottomRight(float horizontalAmount, float verticalAmount) {
			X -= horizontalAmount;
			Y -= verticalAmount;
			Width += horizontalAmount;
			Height += verticalAmount;
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the specified point.  In general, the corners of the rectangle will move away from the anchor point.
		/// </summary>
		public void InflateFromAnchor(Vector2 anchor, float horizontalAmount, float verticalAmount) {
			// Move the corners away from the anchor

			if (Width > 0 && horizontalAmount != 0) {
				float x1x2 = Width;
				float x1x3 = anchor.X - X;

				float x1x3Norm = x1x3 / x1x2;

				if (x1x3Norm > 1) {
					// Anchor is not within the rectangle and is to the right of the top-right corner
					X -= horizontalAmount;
					Width += horizontalAmount;
				} else if (x1x3Norm < 0) {
					// Anchor is not within the rectangle and is to the left of the top-left corner
					X += horizontalAmount;
					Width -= horizontalAmount;
				} else {
					// Anchor is within the rectangle's bounds horizontally
					float midpoint = X + Width / 2;
					if (anchor.X < midpoint) {
						// Anchor is closer to the top-left corner
						X -= horizontalAmount * x1x3Norm;
					} else {
						// Anchor is closer to the top-right corner
						X -= horizontalAmount * (1 - x1x3Norm);
					}

					Width += horizontalAmount;
				}
			}

			if (Height > 0 && verticalAmount != 0) {
				float y1y2 = Height;
				float y1y3 = anchor.Y - Y;
				float y2y3 = Y + Height - anchor.Y;

				float y1y3Norm = y1y3 / y1y2;
				float y2y3Norm = y2y3 / y1y2;

				if (y1y3Norm > 1) {
					// Anchor is not within the rectangle and is below the bottom-left corner
					Y -= verticalAmount;
					Height += verticalAmount;
				} else if (y1y3Norm < 0) {
					// Anchor is not within the rectangle and is above the top-left corner
					Y += verticalAmount;
					Height -= verticalAmount;
				} else {
					// Anchor is within the rectangle's bounds vertically
					float midpoint = Y + Height / 2;
					if (anchor.Y < midpoint) {
						// Anchor is closer to the top-left corner
						Y -= verticalAmount * y1y3Norm;
					} else {
						// Anchor is closer to the bottom-left corner
						Y -= verticalAmount * (1 - y2y3Norm);
					}

					Height += verticalAmount;
				}
			}
		}

		/// <summary>
		/// Determines if the rectangle contains the specified point
		/// </summary>
		public bool Contains(Vector2 point) {
			return point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
		}

		/// <summary/>
		public static implicit operator RectangleF(Rectangle rectangle) {
			return new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}
	}
}
