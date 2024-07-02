using System;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An enumeration of alignment options for a UI element within its parent element
	/// </summary>
	[Flags]
	public enum LayoutGravityType : byte {
		/// <summary>
		/// No alignment
		/// </summary>
		None = 0x00,
		/// <summary/>
		HasHorizontalAlignment = 0x01,
		/// <summary/>
		HasVerticalAlignment = 0x02,
		/// <summary/>
		MirroredHorizontalAlignment = 0x04,
		/// <summary/>
		MirroredVerticalAlignment = 0x08,
		/// <summary>
		/// The left edge of the element is aligned with the left edge of the parent
		/// </summary>
		Left = HasHorizontalAlignment,
		/// <summary>
		/// The right edge of the element is aligned with the right edge of the parent
		/// </summary>
		Right = HasHorizontalAlignment | MirroredHorizontalAlignment,
		/// <summary>
		/// The top edge of the element is aligned with the top edge of the parent
		/// </summary>
		Top = HasVerticalAlignment,
		/// <summary>
		/// The bottom edge of the element is aligned with the bottom edge of the parent
		/// </summary>
		Bottom = HasVerticalAlignment | MirroredVerticalAlignment,
		/// <summary>
		/// The element is centered horizontally within the parent.  This value overrides <see cref="Left"/> and <see cref="Right"/>
		/// </summary>
		CenterHorizontal = 0x10 | HasHorizontalAlignment,
		/// <summary>
		/// The element is centered vertically within the parent.  This value overrides <see cref="Top"/> and <see cref="Bottom"/>
		/// </summary>
		CenterVertical = 0x20 | HasVerticalAlignment,
		/// <summary>
		/// The element is centered both horizontally and vertically within the parent.  This value overrides <see cref="Left"/>, <see cref="Right"/>, <see cref="Top"/>, and <see cref="Bottom"/>
		/// </summary>
		CenterBoth = CenterHorizontal | CenterVertical
	}
}
