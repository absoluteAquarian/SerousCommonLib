namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An interface representing an object that manages organizing UI elements within a parent element
	/// </summary>
	public interface IConstraintLayout {
		/// <summary>
		/// The layout manager for this layout
		/// </summary>
		LayoutManager Manager { get; }
	}
}
