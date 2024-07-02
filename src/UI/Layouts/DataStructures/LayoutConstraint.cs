using SerousCommonLib.API;
using System;
using Terraria.UI;

#nullable enable
namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A structure representing a layout constraint for a UI element with respect to another element
	/// </summary>
	public readonly struct LayoutConstraint {
		/// <summary>
		/// The alignment offset.  Positive offsets are directed upwards for Bottom constraints and to the right for Right constraints.
		/// </summary>
		public readonly LayoutUnit dimension;
		/// <summary>
		/// The type of constraint
		/// </summary>
		public readonly LayoutConstraintType type;
		/// <summary>
		/// The anchor element for the constraint.  If <see langword="null"/>, the element's parent is used.
		/// </summary>
		public readonly WeakReference<UIElement>? anchor;

		internal LayoutConstraint(UIElement? anchor, LayoutUnit dimension, LayoutConstraintType type) {
			this.anchor = anchor.AsWeakReference();
			this.dimension = dimension;
			this.type = type;
		}

		/// <summary>
		/// Attempts to get the anchor element for the constraint.  If the anchor is <see langword="null"/>, the element's parent is used.
		/// </summary>
		public bool TryGetAnchor(UIElement element, out UIElement? anchor) {
			if (this.anchor is null) {
				anchor = element.Parent;
				return true;
			}

			return this.anchor.TryGetTarget(out anchor);
		}
	}
}
