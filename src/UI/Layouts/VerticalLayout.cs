namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A layout that arranges elements vertically
	/// </summary>
	public class VerticalLayout : BaseOrderedLayout {
		/// <inheritdoc/>
		public override LayoutConstraintType AlignmentToParent => LayoutConstraintType.TopToTopOf;

		/// <inheritdoc/>
		public override LayoutConstraintType AlignmentToSibling => LayoutConstraintType.TopToBottomOf;
	}
}
