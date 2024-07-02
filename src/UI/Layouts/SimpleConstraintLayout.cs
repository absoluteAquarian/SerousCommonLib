using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A simple constraint layout that does not have any specific layout behavior
	/// </summary>
	public class SimpleConstraintLayout : UIElement, IConstraintLayout {
		/// <inheritdoc/>
		public LayoutManager Manager { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="SimpleConstraintLayout"/> class
		/// </summary>
		public SimpleConstraintLayout() {
			Manager = LayoutManager.GetOrCreateManager(this);
			Manager.Attributes = new LayoutAttributes();
		}
	}
}
