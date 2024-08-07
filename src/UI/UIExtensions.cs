﻿using SerousCommonLib.UI.Layouts;
using System;
using Terraria.UI;

namespace SerousCommonLib.UI {
	/// <inheritdoc/>
	public static class UIExtensions {
		/// <summary>
		/// Gets the layout manager of the specified element
		/// </summary>
		/// <param name="element">The UI element</param>
		/// <param name="mode">How to handle creating a layout manager if one does not exist for the UI element</param>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public static LayoutManager GetLayoutManager(this UIElement element, LayoutCreationMode mode = LayoutCreationMode.CreateNew) {
			return mode switch {
				LayoutCreationMode.CreateNew => LayoutManager.GetOrCreateManager(element),
				LayoutCreationMode.View => LayoutManager.GetManager(element),
				_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Specified mode does not exist")
			};
		}
	}
}
