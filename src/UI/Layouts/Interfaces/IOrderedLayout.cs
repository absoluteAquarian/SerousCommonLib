namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An interface representing an object that manages organizing UI elements within a parent element with specific alignment rules
	/// </summary>
	public interface IOrderedLayout : IConstraintLayout {
		/// <summary>
		/// The alignment of the first element with respect to the parent element
		/// </summary>
		LayoutConstraintType AlignmentToParent { get; }

		/// <summary>
		/// The alignment of the elements after the first element with respect to the previously placed element
		/// </summary>
		LayoutConstraintType AlignmentToSibling { get; }

		/// <summary>
		/// The spacing between elements in the layout
		/// </summary>
		LayoutUnit Spacing { get; }
	}
}
