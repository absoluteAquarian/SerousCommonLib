﻿using System;
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
		/// The biases for the <see cref="Constraints"/> of the UI element
		/// </summary>
		public ConstraintBias Bias { get; set; } = new ConstraintBias(0.5f, 0.5f);

		/// <summary>
		/// Sets the alignment of the element with respect to its parent's bounds
		/// </summary>
		/// <param name="gravity">The alignment type</param>
		public LayoutAttributes WithGravity(LayoutGravity gravity) {
			Gravity = gravity;
			return this;
		}

		/// <summary>
		/// Sets the alignment of the element with respect to its parent's bounds
		/// </summary>
		/// <param name="type">The gravity type</param>
		/// <param name="horizontal">The horizontal offset</param>
		/// <param name="vertical">The vertical offset</param>
		public LayoutAttributes WithGravity(LayoutGravityType type, LayoutUnit horizontal = default, LayoutUnit vertical = default) {
			Gravity = new LayoutGravity(type, horizontal, vertical);
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
		/// Sets the biases for the <see cref="Constraints"/> of the UI element
		/// </summary>
		/// <param name="bias">The biases</param>
		public LayoutAttributes WithBias(ConstraintBias bias) {
			Bias = bias;
			return this;
		}

		/// <summary>
		/// Sets the biases for the <see cref="Constraints"/> of the UI element
		/// </summary>
		/// <param name="horizontalBias">The horizontal bias</param>
		/// <param name="verticalBias">The vertical bias</param>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public LayoutAttributes WithBias(float horizontalBias, float verticalBias) {
			Bias = new ConstraintBias(horizontalBias, verticalBias);
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

		private WeakReference<UIElement>? _inheritSizeElement, _inheritGravityElement;

		/// <summary>
		/// Inherit the size constraints from another UI element.<br/>
		/// Using this method will override any changes to <see cref="Size"/>
		/// </summary>
		/// <param name="element">The element to copy data from</param>
		public LayoutAttributes InheritSizeFrom(UIElement element) {
			_inheritSizeElement = new WeakReference<UIElement>(element);
			SetSizeFromInheritance(element);
			return this;
		}

		private void SetSizeFromInheritance(UIElement element) {
			Size = new SizeConstraint(element.Width, element.Height, element.MinWidth, element.MaxWidth, element.MinHeight, element.MaxHeight);
		}

		/// <summary>
		/// Inherit the <see cref="UIElement.HAlign"/> and <see cref="UIElement.VAlign"/> constraints from another UI element.<br/>
		/// Using this method will override any changes to <see cref="Gravity"/>
		/// </summary>
		/// <param name="element">The element to copy data from</param>
		public LayoutAttributes InheritGravityFrom(UIElement element) {
			_inheritGravityElement = new WeakReference<UIElement>(element);
			SetGravityFromInheritance(element);
			return this;
		}

		private void SetGravityFromInheritance(UIElement element) {
			LayoutGravityType type = LayoutGravityType.None;
			
			if (element.HAlign != 0)
				type |= LayoutGravityType.CenterHorizontal;
			if (element.VAlign != 0)
				type |= LayoutGravityType.CenterVertical;

			Gravity = new LayoutGravity(type, new LayoutUnit(percent: element.HAlign), new LayoutUnit(percent: element.VAlign));
		}

		internal void CheckInheritance() {
			if (_inheritSizeElement?.TryGetTarget(out UIElement? element) is true)
				SetSizeFromInheritance(element);

			if (_inheritGravityElement?.TryGetTarget(out element) is true)
				SetGravityFromInheritance(element);
		}
	}
}
