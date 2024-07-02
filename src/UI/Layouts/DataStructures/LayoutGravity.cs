using Microsoft.Xna.Framework;
using System;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A structure representing a layout gravity for a UI element within its parent element
	/// </summary>
	public readonly struct LayoutGravity {
		/// <summary>
		/// The alignment type of the layout gravity
		/// </summary>
		public readonly LayoutGravityType type;
		/// <summary>
		/// The horizontal offset of the layout gravity if applicable.  For <see cref="LayoutGravityType.Right"/>, positive offsets are directed to the left.
		/// </summary>
		public readonly LayoutUnit horizontal;
		/// <summary>
		/// The vertical offset of the layout gravity if applicable.  For <see cref="LayoutGravityType.Bottom"/>, positive offsets are directed upwards.
		/// </summary>
		public readonly LayoutUnit vertical;

		/// <summary>
		/// Creates a new layout gravity with the specified type and offsets.<br/>
		/// If the gravity type does not have a horizontal or vertical alignment, the respective offset will be ignored.
		/// </summary>
		/// <param name="type">The gravity type</param>
		/// <param name="horizontal">The horizontal offset</param>
		/// <param name="vertical">The vertical offset</param>
		public LayoutGravity(LayoutGravityType type, LayoutUnit horizontal = default, LayoutUnit vertical = default) {
			// Centered alignments override the other alignment types on their axes
			if ((type & LayoutGravityType.CenterHorizontal) != 0)
				type &= ~(LayoutGravityType.HasHorizontalAlignment | LayoutGravityType.MirroredHorizontalAlignment);
			if ((type & LayoutGravityType.CenterVertical) != 0)
				type &= ~(LayoutGravityType.HasVerticalAlignment | LayoutGravityType.MirroredVerticalAlignment);

			this.type = type;

			if ((type & (LayoutGravityType.HasHorizontalAlignment | LayoutGravityType.CenterHorizontal)) == 0)
				horizontal = default;
			if ((type & (LayoutGravityType.HasVerticalAlignment | LayoutGravityType.CenterVertical)) == 0)
				vertical = default;

			this.horizontal = horizontal;
			this.vertical = vertical;
		}

		/// <summary>
		/// Calculates the anchor position of the layout gravity within the parent element
		/// </summary>
		public Vector2 GetAnchor(Vector2 parentSize) {
			Vector2 anchor = default;

			if ((type & LayoutGravityType.CenterBoth) != 0) {
				if ((type & LayoutGravityType.CenterHorizontal) != 0)
					anchor.X = parentSize.X / 2;
				else if ((type & LayoutGravityType.CenterVertical) != 0)
					anchor.Y = parentSize.Y / 2;
				else
					anchor = parentSize / 2;
			} else {
				if ((type & LayoutGravityType.HasHorizontalAlignment) != 0)
					anchor.X = (type & LayoutGravityType.MirroredHorizontalAlignment) == 0 ? 0 : parentSize.X;
				if ((type & LayoutGravityType.HasVerticalAlignment) != 0)
					anchor.Y = (type & LayoutGravityType.MirroredVerticalAlignment) == 0 ? 0 : parentSize.Y;
			}

			anchor.X += horizontal.GetValueRaw(parentSize.X);
			anchor.Y += vertical.GetValueRaw(parentSize.Y);

			return anchor;
		}
	}
}
