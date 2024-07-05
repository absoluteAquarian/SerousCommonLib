using Microsoft.Xna.Framework;
using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	internal class LayoutDimensionLink {
		private readonly CalculatedLayout _depdendent;
		private readonly CalculatedLayout _anchor;

		private readonly LayoutUnit _offset;
		private readonly LayoutConstraintType _type;

		public LayoutDimensionLink(CalculatedLayout dependent, CalculatedLayout anchor, LayoutConstraint depdendentConstraint) {
			_depdendent = dependent;
			_anchor = anchor;
			_offset = depdendentConstraint.dimension;
			_type = depdendentConstraint.type;
		}

		public void Evaluate() {
			CalculatedLayout parent = _depdendent.Parent ?? CalculatedLayout.Screen;
			Vector2 parentSize = parent.GetChildContainerSize();
			bool anchorHasElement = _anchor.TryGetElement(out UIElement anchorElement);

			switch (_type) {
				case LayoutConstraintType.LeftToLeftOf:
					_depdendent.Left = _anchor.Left + _offset.GetValueRaw(parentSize.X);
					if (anchorHasElement)
						_depdendent.Left += anchorElement.PaddingLeft;
					break;
				case LayoutConstraintType.LeftToRightOf:
					_depdendent.Left = _anchor.Right + _offset.GetValueRaw(parentSize.X);
					break;
				case LayoutConstraintType.RightToLeftOf:
					_depdendent.Right = _anchor.Left - _offset.GetValueRaw(parentSize.X);
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
					break;
				case LayoutConstraintType.BottomToTopOf:
					_depdendent.Bottom = _anchor.Top - _offset.GetValueRaw(parentSize.Y);
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
