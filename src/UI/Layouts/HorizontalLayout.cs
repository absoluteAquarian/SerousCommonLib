using System;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A layout that arranges elements horizontally
	/// </summary>
	[Obsolete("This class is part of a new API that is not yet complete.", error: true)]
	public class HorizontalLayout : BaseOrderedLayout {
		/// <inheritdoc/>
		public override LayoutConstraintType AlignmentToParent => LayoutConstraintType.LeftToLeftOf;

		/// <inheritdoc/>
		public override LayoutConstraintType AlignmentToSibling => LayoutConstraintType.LeftToRightOf;
	}
}
