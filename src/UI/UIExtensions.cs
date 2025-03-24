using SerousCommonLib.UI.Layouts;
using System;
using Terraria.UI;

namespace SerousCommonLib.UI {
	/// <summary>
	/// Extension methods for <see cref="UIElement"/>
	/// </summary>
	public static class UIExtensions {
		/// <summary>
		/// Gets the layout manager of the specified element
		/// </summary>
		/// <param name="element">The UI element</param>
		/// <param name="mode">How to handle creating a layout manager if one does not exist for the UI element</param>
		/// <exception cref="ArgumentOutOfRangeException"/>
		[Obsolete("This method is part of a new API that is not yet complete.", error: true)]
		public static LayoutManager GetLayoutManager(this UIElement element, LayoutCreationMode mode = LayoutCreationMode.CreateNew) {
			ArgumentNullException.ThrowIfNull(element);

			return mode switch {
				LayoutCreationMode.CreateNew => LayoutManager.GetOrCreateManager(element),
				LayoutCreationMode.View => LayoutManager.GetManager(element),
				_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Specified mode does not exist")
			};
		}

		/// <summary>
		/// This method modifies <see cref="UIElement.Left"/> and <see cref="UIElement.HAlign"/> to set an effective alignment of <paramref name="element"/>'s right edge to the right edge of its parent
		/// </summary>
		/// <param name="element">The child element</param>
		/// <param name="offset">The offset from the parent's right edge.  Positive values move <paramref name="element"/> toward the left edge of its parent.</param>
		public static void SetRightAlignment(this UIElement element, float offset) {
			element.Left.Set(-offset, 0f);
			element.HAlign = 1f;
		}

		/// <summary>
		/// This method modifies <see cref="UIElement.Top"/> and <see cref="UIElement.VAlign"/> to set an effective alignment of <paramref name="element"/>'s bottom edge to the bottom edge of its parent
		/// </summary>
		/// <param name="element">The child element</param>
		/// <param name="offset">The offset from the parent's bottom edge.  Positive values move <paramref name="element"/> toward the top edge of its parent.</param>
		public static void SetBottomAlignment(this UIElement element, float offset) {
			element.Top.Set(-offset, 0f);
			element.VAlign = 1f;
		}
	}
}
