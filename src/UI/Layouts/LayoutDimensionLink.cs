using Microsoft.Xna.Framework;

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
			Vector2 parentSize = new Vector2(parent.Width, parent.Height);

			switch (_type) {
				case LayoutConstraintType.LeftToLeftOf:
					_depdendent.Left = _anchor.Left + _offset.GetValueRaw(parentSize.X);
					break;
				case LayoutConstraintType.LeftToRightOf:
					_depdendent.Left = _anchor.Right + _offset.GetValueRaw(parentSize.X);
					break;
				case LayoutConstraintType.RightToLeftOf:
					_depdendent.Right = _anchor.Left - _offset.GetValueRaw(parentSize.X);
					break;
				case LayoutConstraintType.RightToRightOf:
					_depdendent.Right = _anchor.Right - _offset.GetValueRaw(parentSize.X);
					break;
				case LayoutConstraintType.TopToTopOf:
					_depdendent.Top = _anchor.Top + _offset.GetValueRaw(parentSize.Y);
					break;
				case LayoutConstraintType.TopToBottomOf:
					_depdendent.Top = _anchor.Bottom + _offset.GetValueRaw(parentSize.Y);
					break;
				case LayoutConstraintType.BottomToTopOf:
					_depdendent.Bottom = _anchor.Top - _offset.GetValueRaw(parentSize.Y);
					break;
				case LayoutConstraintType.BottomToBottomOf:
					_depdendent.Bottom = _anchor.Bottom - _offset.GetValueRaw(parentSize.Y);
					break;
			}
		}
	}
}
