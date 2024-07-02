using System;
using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A structure representing the size constraints for a UI element
	/// </summary>
	public readonly struct SizeConstraint {
		/// <summary>
		/// The desired width of the UI element before considering <see cref="minWidth"/> and <see cref="maxWidth"/>
		/// </summary>
		public readonly LayoutUnit width;
		/// <summary>
		/// The desired height of the UI element before considering <see cref="minHeight"/> and <see cref="maxHeight"/>
		/// </summary>
		public readonly LayoutUnit height;

		/// <summary>
		/// The minimum width of the UI element
		/// </summary>
		public readonly LayoutUnit minWidth;
		/// <summary>
		/// The maximum width of the UI element
		/// </summary>
		public readonly LayoutUnit maxWidth;
		/// <summary>
		/// The minimum height of the UI element
		/// </summary>
		public readonly LayoutUnit minHeight;
		/// <summary>
		/// The maximum height of the UI element
		/// </summary>
		public readonly LayoutUnit maxHeight;

		/// <summary>
		/// Creates a new instance of <see cref="SizeConstraint"/> with the provided width and height
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public SizeConstraint(LayoutUnit width, LayoutUnit height, LayoutUnit? minWidth = default, LayoutUnit? maxWidth = default, LayoutUnit? minHeight = default, LayoutUnit? maxHeight = default) {
			this.width = width;
			this.height = height;
			this.minWidth = minWidth ?? LayoutUnit.Zero;
			this.maxWidth = maxWidth ?? LayoutUnit.Fill;
			this.minHeight = minHeight ?? LayoutUnit.Zero;
			this.maxHeight = maxHeight ?? LayoutUnit.Fill;
		}

		/// <summary>
		/// Gets the width of this constraint clamped by <see cref="minWidth"/> and <see cref="maxWidth"/>
		/// </summary>
		public float GetConstrainedWidth(float parentWidth) => LayoutUnit.Clamp(width.GetValueRaw(parentWidth), minWidth, maxWidth, parentWidth);

		/// <summary>
		/// Gets the height of this constraint clamped by <see cref="minHeight"/> and <see cref="maxHeight"/>
		/// </summary>
		/// <param name="parentHeight"></param>
		/// <returns></returns>
		public float GetConstrainedHeight(float parentHeight) => LayoutUnit.Clamp(height.GetValueRaw(parentHeight), minHeight, maxHeight, parentHeight);

		/// <summary>
		/// Clamps the provided width by <see cref="minWidth"/> and <see cref="maxWidth"/>
		/// </summary>
		public float ClampWidth(float width, float parentWidth) => LayoutUnit.Clamp(width, minWidth, maxWidth, parentWidth);

		/// <summary>
		/// Clamps the provided height by <see cref="minHeight"/> and <see cref="maxHeight"/>
		/// </summary>
		public float ClampHeight(float height, float parentHeight) => LayoutUnit.Clamp(height, minHeight, maxHeight, parentHeight);
	}
}
