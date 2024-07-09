using Microsoft.Xna.Framework;
using SerousCommonLib.API;
using System;

namespace SerousCommonLib.UI.Layouts {
	/// <summary/>
	public static class LayoutExtensions {
		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the specified relative layout coordinate
		/// </summary>
		public static void InflateWithLayout(this RectangleF rectangle, float horizontalAmount, float verticalAmount, LayoutUnit horizontalAlignment, LayoutUnit verticalAlignment, Vector2 parentSize) {
			float relativeX = horizontalAlignment.GetValueRaw(parentSize.X);
			float relativeY = verticalAlignment.GetValueRaw(parentSize.Y);

			Vector2 anchor = new Vector2(rectangle.X + relativeX, rectangle.Y + relativeY);

			rectangle.InflateFromAnchor(anchor, horizontalAmount, verticalAmount);
		}

		/// <summary>
		/// Increases the size of the rectangle by the specified amounts, anchored around the specified layout gravity's anchor position
		/// </summary>
		public static void InflateWithLayout(this RectangleF rectangle, float horizontalAmount, float verticalAmount, in LayoutGravity gravity, Vector2 parentSize) {
			Vector2 anchor = gravity.GetAnchor(parentSize);

			rectangle.InflateFromAnchor(anchor, horizontalAmount, verticalAmount);
		}

		internal static LayoutEdge GetSourceEdgeFromConstraint(this LayoutConstraintType type) {
			return type switch {
				LayoutConstraintType.LeftToLeftOf => LayoutEdge.Left,
				LayoutConstraintType.LeftToRightOf => LayoutEdge.Left,
				LayoutConstraintType.RightToLeftOf => LayoutEdge.Right,
				LayoutConstraintType.RightToRightOf => LayoutEdge.Right,
				LayoutConstraintType.TopToTopOf => LayoutEdge.Top,
				LayoutConstraintType.TopToBottomOf => LayoutEdge.Top,
				LayoutConstraintType.BottomToTopOf => LayoutEdge.Bottom,
				LayoutConstraintType.BottomToBottomOf => LayoutEdge.Bottom,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid constraint type")
			};
		}

		internal static LayoutEdge GetAnchorEdgeFromConstraint(this LayoutConstraintType type) {
			return type switch {
				LayoutConstraintType.LeftToLeftOf => LayoutEdge.Left,
				LayoutConstraintType.LeftToRightOf => LayoutEdge.Right,
				LayoutConstraintType.RightToLeftOf => LayoutEdge.Left,
				LayoutConstraintType.RightToRightOf => LayoutEdge.Right,
				LayoutConstraintType.TopToTopOf => LayoutEdge.Top,
				LayoutConstraintType.TopToBottomOf => LayoutEdge.Bottom,
				LayoutConstraintType.BottomToTopOf => LayoutEdge.Top,
				LayoutConstraintType.BottomToBottomOf => LayoutEdge.Bottom,
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid constraint type")
			};
		}
	}
}
