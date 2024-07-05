namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An enum representing the different ways a layout can be created when using <see cref="UIExtensions.GetLayoutManager"/>
	/// </summary>
	public enum LayoutCreationMode {
		/// <summary>
		/// Creates a new layout manager for the UI element if one does not already exist
		/// </summary>
		CreateNew,
		/// <summary>
		/// Gets the existing layout manager for the UI element if one exists, or creates a read-only layout manager otherwise.
		/// </summary>
		View
	}
}
