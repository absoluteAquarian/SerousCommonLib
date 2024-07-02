namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A layout that arranges elements horizontally
	/// </summary>
	public class HorizontalLayout : BaseOrderedLayout {
		/// <inheritdoc/>
		public override LayoutConstraintType AlignmentToParent => LayoutConstraintType.LeftToLeftOf;

		/// <inheritdoc/>
		public override LayoutConstraintType AlignmentToSibling => LayoutConstraintType.LeftToRightOf;
	}
}
