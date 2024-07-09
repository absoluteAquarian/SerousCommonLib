using System.Collections.Generic;
using Terraria.UI;

#nullable enable
namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An object containing the gravity and layout constraints for a UI element
	/// </summary>
	public class LayoutAttributes {
		/// <summary>
		/// The alignment of the element with respect to its parent's bounds
		/// </summary>
		public LayoutGravity Gravity { get; set; }

		private readonly Dictionary<LayoutEdge, LayoutConstraint> _constraintByType = [];
		/// <summary>
		/// The layout constraints for the UI element
		/// </summary>
		public IEnumerable<LayoutConstraint> Constraints => _constraintByType.AsReadOnly().Values;

		/// <summary>
		/// The desired size of the UI element.  If <see langword="null"/>, the element will not attempt to resize itself.
		/// </summary>
		public SizeConstraint? Size { get; set; }

		/// <summary>
		/// Sets the alignment of the element with respect to its parent's bounds
		/// </summary>
		/// <param name="gravity">The alignment type</param>
		public LayoutAttributes WithGravity(LayoutGravity gravity) {
			Gravity = gravity;
			return this;
		}

		/// <summary>
		/// Sets the desired size of the UI element
		/// </summary>
		/// <param name="size">The size to attempt to resize to</param>
		public LayoutAttributes WithSize(SizeConstraint size) {
			Size = size;
			return this;
		}

		/// <summary>
		/// Sets the desired size of the UI element
		/// </summary>
		/// <param name="width">The width to try to resize to.</param>
		/// <param name="height">The height to try to resize to</param>
		/// <param name="minWidth">The minimum width the element can be resized to</param>
		/// <param name="maxWidth">The maximum width the element can be resized to</param>
		/// <param name="minHeight">The minimum height the element can be resized to</param>
		/// <param name="maxHeight">The maximum height the element can be resized to</param>
		public LayoutAttributes WithSize(LayoutUnit width, LayoutUnit height, LayoutUnit? minWidth = default, LayoutUnit? maxWidth = default, LayoutUnit? minHeight = default, LayoutUnit? maxHeight = default) {
			Size = new SizeConstraint(width, height, minWidth, maxWidth, minHeight, maxHeight);
			return this;
		}

		/// <summary>
		/// Adds a layout constraint to the UI element.  If a constraint of the same type already exists, it is overwritten.
		/// </summary>
		/// <param name="type">Which alignment type to use</param>
		/// <param name="anchor">Which element to use as a reference for alignment.  If <see langword="null"/>, the element's parent is used.</param>
		/// <param name="dimension">The offset of the alignment</param>
		public LayoutAttributes AddConstraint(LayoutConstraintType type, UIElement? anchor, LayoutUnit dimension) {
			_constraintByType[type.GetSourceEdgeFromConstraint()] = new LayoutConstraint(anchor, dimension, type);
			return this;
		}

		/// <summary>
		/// Removes a layout constraint from the UI element, if it exists
		/// </summary>
		public LayoutAttributes RemoveConstraint(LayoutConstraintType type) {
			_constraintByType.Remove(type.GetSourceEdgeFromConstraint());
			return this;
		}

		/// <summary>
		/// Changes the anchor of a layout constraint.  If the constraint does not exist, nothing happens.
		/// </summary>
		/// <param name="type">Which alignment type to use</param>
		/// <param name="anchor">Which element to use as a reference for alignment.  If <see langword="null"/>, the element's parent is used.</param>
		public LayoutAttributes ChangeConstraintAnchor(LayoutConstraintType type, UIElement? anchor) {
			var edge = type.GetSourceEdgeFromConstraint();

			if (_constraintByType.TryGetValue(edge, out LayoutConstraint constraint))
				_constraintByType[edge] = new LayoutConstraint(anchor, constraint.dimension, type);

			return this;
		}

		/// <summary>
		/// Clears all constraints from the UI element
		/// </summary>
		public LayoutAttributes ClearConstraints() {
			_constraintByType.Clear();
			return this;
		}
	}
}
