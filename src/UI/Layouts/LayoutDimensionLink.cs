using Microsoft.Xna.Framework;
using System;
using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	internal class LayoutDimensionLink {
		public readonly CalculatedLayout depdendent;
		public readonly CalculatedLayout anchor;

		public readonly LayoutUnit offset;
		public readonly LayoutConstraintType type;

		public LayoutDimensionLink oppositeSide;

		public LayoutDimensionLink(CalculatedLayout dependent, CalculatedLayout anchor, LayoutConstraint depdendentConstraint) {
			if (object.ReferenceEquals(dependent, anchor))
				throw new ArgumentException("Layout constraint cannot target the same layout as the one it is assigned to");

			depdendent = dependent;
			this.anchor = anchor;
			offset = depdendentConstraint.dimension;
			type = depdendentConstraint.type;
		}

		public void Evaluate() {
			Vector2 parentSize = depdendent.Parent.GetChildContainerSize();
			bool anchorHasElement = anchor.TryGetElement(out UIElement anchorElement);

			switch (type) {
				case LayoutConstraintType.LeftToLeftOf:
					depdendent.Left = anchor.Left + GetHorizontalOffset(parentSize.X);
					if (anchorHasElement)
						depdendent.Left += anchorElement.PaddingLeft;
					break;
				case LayoutConstraintType.LeftToRightOf:
					depdendent.Left = anchor.Right + GetHorizontalOffset(parentSize.X);
					if (anchorHasElement)
						depdendent.Left += anchorElement.MarginRight;
					break;
				case LayoutConstraintType.RightToLeftOf:
					depdendent.Right = anchor.Left - GetHorizontalOffset(parentSize.X);
					if (anchorHasElement)
						depdendent.Right -= anchorElement.MarginLeft;
					break;
				case LayoutConstraintType.RightToRightOf:
					depdendent.Right = anchor.Right - GetHorizontalOffset(parentSize.X);
					if (anchorHasElement)
						depdendent.Right -= anchorElement.PaddingRight;
					break;
				case LayoutConstraintType.TopToTopOf:
					depdendent.Top = anchor.Top + GetVerticalOffset(parentSize.Y);
					if (anchorHasElement)
						depdendent.Top += anchorElement.PaddingTop;
					break;
				case LayoutConstraintType.TopToBottomOf:
					depdendent.Top = anchor.Bottom + GetVerticalOffset(parentSize.Y);
					if (anchorHasElement)
						depdendent.Top += anchorElement.MarginBottom;
					break;
				case LayoutConstraintType.BottomToTopOf:
					depdendent.Bottom = anchor.Top - GetVerticalOffset(parentSize.Y);
					if (anchorHasElement)
						depdendent.Bottom -= anchorElement.MarginTop;
					break;
				case LayoutConstraintType.BottomToBottomOf:
					depdendent.Bottom = anchor.Bottom - GetVerticalOffset(parentSize.Y);
					if (anchorHasElement)
						depdendent.Bottom -= anchorElement.PaddingBottom;
					break;
			}
		}

		private float GetHorizontalOffset(float parentWidth) {
			float self = offset.GetValueRaw(parentWidth);

			if (oppositeSide is null)
				return self;

			float opposite = oppositeSide.offset.GetValueRaw(parentWidth);

			// Get the horizontal bias of the element
			float bias = depdendent.attributes?.Bias.horizontalBias ?? 0.5f;  // Default to center

			if (type is LayoutConstraintType.RightToLeftOf or LayoutConstraintType.RightToRightOf)
				bias = 1 - bias;

			return (self + opposite) * bias;
		}

		private float GetVerticalOffset(float parentHeight) {
			float self = offset.GetValueRaw(parentHeight);

			if (oppositeSide is null)
				return self;

			float opposite = oppositeSide.offset.GetValueRaw(parentHeight);

			// Get the vertical bias of the element
			float bias = depdendent.attributes?.Bias.verticalBias ?? 0.5f;  // Default to center

			if (type is LayoutConstraintType.BottomToTopOf or LayoutConstraintType.BottomToBottomOf)
				bias = 1 - bias;

			return (self + opposite) * bias;
		}
	}
}
