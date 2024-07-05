using Microsoft.Xna.Framework;
using SerousCommonLib.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria.UI;

#nullable enable
namespace SerousCommonLib.UI.Layouts {
	internal class CalculatedLayout {
		private enum ConstraintEdge {
			Left,
			Top,
			Right,
			Bottom
		}

		private CalculatedLayout? _parent;
		private readonly HashSet<CalculatedLayout> _children = [];

		private readonly Dictionary<ConstraintEdge, List<LayoutDimensionLink>> _linksByType = new() {
			[ConstraintEdge.Left] = [],
			[ConstraintEdge.Top] = [],
			[ConstraintEdge.Right] = [],
			[ConstraintEdge.Bottom] = []
		};

		public CalculatedLayout? Parent => _parent;

		private LayoutDimension _left;
		public virtual LayoutDimension Left {
			get => _left;
			set {
				_left = value;
				foreach (LayoutDimensionLink link in _linksByType[ConstraintEdge.Left])
					link.Evaluate();
			}
		}

		private LayoutDimension _top;
		public virtual LayoutDimension Top {
			get => _top;
			set {
				_top = value;
				foreach (LayoutDimensionLink link in _linksByType[ConstraintEdge.Top])
					link.Evaluate();
			}
		}

		private LayoutDimension _right;
		public virtual LayoutDimension Right {
			get => _right;
			set {
				_right = value;
				foreach (LayoutDimensionLink link in _linksByType[ConstraintEdge.Right])
					link.Evaluate();
			}
		}

		private LayoutDimension _bottom;
		public virtual LayoutDimension Bottom {
			get => _bottom;
			set {
				_bottom = value;
				foreach (LayoutDimensionLink link in _linksByType[ConstraintEdge.Bottom])
					link.Evaluate();
			}
		}

		public float Width => Right - Left;

		public float Height => Bottom - Top;

		public RectangleF CalculatedArea => new RectangleF(Left, Top, Width, Height);

		public readonly WeakReference<UIElement>? source;

		public static CalculatedLayout Screen => new ReadOnlyCalculatedLayout(null);

		public CalculatedLayout(UIElement? source, float left = 0, float top = 0, float width = 0, float height = 0) {
			this.source = source.AsWeakReference();
			Left = new LayoutDimension(left);
			Top = new LayoutDimension(top);
			Right = new LayoutDimension(left + width);
			Bottom = new LayoutDimension(top + height);
		}

		public bool TryGetElement([NotNullWhen(true)] out UIElement? element) {
			if (source?.TryGetTarget(out element) is not true) {
				element = null;
				return false;
			}

			return element is not null;
		}

		public CalculatedLayout AssignWidth(float width, in LayoutGravity gravity, Vector2 parentSize) {
			RectangleF area = CalculatedArea;
			area.InflateWithLayout(width - area.Width, 0, gravity, parentSize);

			Left = new LayoutDimension(area.X);
			Right = new LayoutDimension(area.X + width);
			Top = new LayoutDimension(area.Y);
			Bottom = new LayoutDimension(area.Y + area.Height);

			return this;
		}

		public CalculatedLayout AssignHeight(float height, in LayoutGravity gravity, Vector2 parentSize) {
			RectangleF area = CalculatedArea;
			area.InflateWithLayout(0, height - area.Height, gravity, parentSize);

			Left = new LayoutDimension(area.X);
			Right = new LayoutDimension(area.X + area.Width);
			Top = new LayoutDimension(area.Y);
			Bottom = new LayoutDimension(area.Y + height);

			return this;
		}

		public CalculatedLayout AssignParent(CalculatedLayout parent) {
			_parent = parent;
			parent._children.Add(this);
			return this;
		}

		public CalculatedLayout AssignChild(CalculatedLayout child) {
			_children.Add(child);
			child._parent = this;
			return this;
		}

		public CalculatedLayout LinkDimension(CalculatedLayout dependent, LayoutConstraint dependentConstraint) {
			ConstraintEdge edge = dependentConstraint.type switch {
				LayoutConstraintType.LeftToLeftOf => ConstraintEdge.Left,
				LayoutConstraintType.LeftToRightOf => ConstraintEdge.Right,
				LayoutConstraintType.RightToLeftOf => ConstraintEdge.Left,
				LayoutConstraintType.RightToRightOf => ConstraintEdge.Right,
				LayoutConstraintType.TopToTopOf => ConstraintEdge.Top,
				LayoutConstraintType.TopToBottomOf => ConstraintEdge.Bottom,
				LayoutConstraintType.BottomToTopOf => ConstraintEdge.Top,
				LayoutConstraintType.BottomToBottomOf => ConstraintEdge.Bottom,
				_ => throw new ArgumentOutOfRangeException($"{nameof(dependentConstraint)}.{nameof(dependentConstraint.type)}")
			};

			_linksByType[edge].Add(new LayoutDimensionLink(dependent, this, dependentConstraint));

			return this;
		}

		internal void EvaluateHorizontalConstraints() {
			foreach (LayoutDimensionLink link in _linksByType[ConstraintEdge.Left])
				link.Evaluate();
			foreach (LayoutDimensionLink link in _linksByType[ConstraintEdge.Right])
				link.Evaluate();
		}

		internal void EvaluateVerticalConstraints() {
			foreach (LayoutDimensionLink link in _linksByType[ConstraintEdge.Top])
				link.Evaluate();
			foreach (LayoutDimensionLink link in _linksByType[ConstraintEdge.Bottom])
				link.Evaluate();
		}

		public Vector2 GetChildContainerSize() {
			Vector2 size = new Vector2(Width, Height);
			if (!TryGetElement(out var self))
				return size;

			size.X -= self.MarginLeft + self.MarginRight + self.PaddingLeft + self.PaddingRight;
			size.Y -= self.MarginTop + self.MarginBottom + self.PaddingTop + self.PaddingBottom;

			if (size.X < 0)
				size.X = 0;
			if (size.Y < 0)
				size.Y = 0;

			return size;
		}

		public virtual void ToTerrariaDimensions(UIElement element, out CalculatedStyle innerDims, out CalculatedStyle dims, out CalculatedStyle outerDims) {
			float x = Left;
			float y = Top;
			float width = Width;
			float height = Height;

			outerDims = new CalculatedStyle(x, y, width, height);
			
			x += element.MarginLeft;
			y += element.MarginTop;
			width -= element.MarginLeft + element.MarginRight;
			height -= element.MarginTop + element.MarginBottom;

			dims = new CalculatedStyle(x, y, width, height);

			x += element.PaddingLeft;
			y += element.PaddingTop;
			width -= element.PaddingLeft + element.PaddingRight;
			height -= element.PaddingTop + element.PaddingBottom;

			innerDims = new CalculatedStyle(x, y, width, height);
		}
	}
}
