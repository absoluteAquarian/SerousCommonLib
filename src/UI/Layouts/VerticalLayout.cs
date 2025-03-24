using System;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A layout that arranges elements vertically
	/// </summary>
	[Obsolete("This class is part of a new API that is not yet complete.", error: true)]
	public class VerticalLayout : BaseOrderedLayout {
		/// <inheritdoc/>
		public override LayoutConstraintType AlignmentToParent => LayoutConstraintType.TopToTopOf;

		/// <inheritdoc/>
		public override LayoutConstraintType AlignmentToSibling => LayoutConstraintType.TopToBottomOf;
	}
}
