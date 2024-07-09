using Microsoft.Xna.Framework;
using System;
using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	internal class LayoutDimensionLink {
		private readonly CalculatedLayout _depdendent;
		private readonly CalculatedLayout _anchor;

		private readonly LayoutUnit _offset;
		private readonly LayoutConstraintType _type;

		public LayoutDimensionLink(CalculatedLayout dependent, CalculatedLayout anchor, LayoutConstraint depdendentConstraint) {
			if (object.ReferenceEquals(dependent, anchor))
				throw new ArgumentException("Layout constraint cannot target the same layout as the one it is assigned to");

			_depdendent = dependent;
			_anchor = anchor;
			_offset = depdendentConstraint.dimension;
			_type = depdendentConstraint.type;
		}

		public void Evaluate() {
			Vector2 parentSize = (_depdendent.Parent ?? CalculatedLayout.GetScreenLayout()).GetChildContainerSize();
			bool anchorHasElement = _anchor.TryGetElement(out UIElement anchorElement);

			switch (_type) {
				case LayoutConstraintType.LeftToLeftOf:
					_depdendent.Left = _anchor.Left + _offset.GetValueRaw(parentSize.X);
					if (anchorHasElement)
						_depdendent.Left += anchorElement.PaddingLeft;
					break;
				case LayoutConstraintType.LeftToRightOf:
					_depdendent.Left = _anchor.Right + _offset.GetValueRaw(parentSize.X);
					if (anchorHasElement)
						_depdendent.Left += anchorElement.MarginRight;
					break;
				case LayoutConstraintType.RightToLeftOf:
					_depdendent.Right = _anchor.Left - _offset.GetValueRaw(parentSize.X);
					if (anchorHasElement)
						_depdendent.Right -= anchorElement.MarginLeft;
					break;
				case LayoutConstraintType.RightToRightOf:
					_depdendent.Right = _anchor.Right - _offset.GetValueRaw(parentSize.X);
					if (anchorHasElement)
						_depdendent.Right -= anchorElement.PaddingRight;
					break;
				case LayoutConstraintType.TopToTopOf:
					_depdendent.Top = _anchor.Top + _offset.GetValueRaw(parentSize.Y);
					if (anchorHasElement)
						_depdendent.Top += anchorElement.PaddingTop;
					break;
				case LayoutConstraintType.TopToBottomOf:
					_depdendent.Top = _anchor.Bottom + _offset.GetValueRaw(parentSize.Y);
					if (anchorHasElement)
						_depdendent.Top += anchorElement.MarginBottom;
					break;
				case LayoutConstraintType.BottomToTopOf:
					_depdendent.Bottom = _anchor.Top - _offset.GetValueRaw(parentSize.Y);
					if (anchorHasElement)
						_depdendent.Bottom -= anchorElement.MarginTop;
					break;
				case LayoutConstraintType.BottomToBottomOf:
					_depdendent.Bottom = _anchor.Bottom - _offset.GetValueRaw(parentSize.Y);
					if (anchorHasElement)
						_depdendent.Bottom -= anchorElement.PaddingBottom;
					break;
			}
		}
	}
}
