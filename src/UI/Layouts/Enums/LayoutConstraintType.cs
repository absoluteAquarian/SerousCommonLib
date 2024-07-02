namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An enumeration of constraint types for a UI element with respect to another element
	/// </summary>
	public enum LayoutConstraintType : byte {
		/// <summary>
		/// The left edge of the element is aligned with the left edge of the anchor element
		/// </summary>
		LeftToLeftOf,
		/// <summary>
		/// The left edge of the element is aligned with the right edge of the anchor element
		/// </summary>
		LeftToRightOf,
		/// <summary>
		/// The right edge of the element is aligned with the left edge of the anchor element
		/// </summary>
		RightToLeftOf,
		/// <summary>
		/// The right edge of the element is aligned with the right edge of the anchor element
		/// </summary>
		RightToRightOf,
		/// <summary>
		/// The top edge of the element is aligned with the top edge of the anchor element
		/// </summary>
		TopToTopOf,
		/// <summary>
		/// The top edge of the element is aligned with the bottom edge of the anchor element
		/// </summary>
		TopToBottomOf,
		/// <summary>
		/// The bottom edge of the element is aligned with the top edge of the anchor element
		/// </summary>
		BottomToTopOf,
		/// <summary>
		/// The bottom edge of the element is aligned with the bottom edge of the anchor element
		/// </summary>
		BottomToBottomOf
	}
}
