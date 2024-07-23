namespace SerousCommonLib.UI {
	/// <summary>
	/// A variant of <see cref="NewUIScrollbar"/> that scrolls horizontally
	/// </summary>
	public class NewUIScrollBarHorizontal : NewUIScrollbar {
		/// <inheritdoc/>
		public sealed override bool VerticalScrolling => false;
	}
}
