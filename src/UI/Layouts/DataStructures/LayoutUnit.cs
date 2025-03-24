using System;
using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A structure representing a unit of size for a layout metric
	/// </summary>
	public readonly struct LayoutUnit {
		/// <summary>
		/// The offset of the dimension in pixels
		/// </summary>
		public readonly float pixels;
		/// <summary>
		/// The offset of the dimension as a percentage of the parent element's size
		/// </summary>
		public readonly float percent;

		private readonly bool _isDynamic;

		/// <summary>
		/// A layout dimension that is considered dynamic, i.e. the parent layout's size is dependent on the locations and sizes of its children.<br/>
		/// If this layout unit is used as a constraint, it is instead just equivalent to <see cref="Zero"/>
		/// </summary>
		public static readonly LayoutUnit DynamicSize = new LayoutUnit(0, 0, true);

		/// <summary>
		/// A layout dimension with a zero offset
		/// </summary>
		public static readonly LayoutUnit Zero = default;

		/// <summary>
		/// A layout dimension that fills the parent element's size
		/// </summary>
		public static readonly LayoutUnit Fill = new LayoutUnit(0, 1);

		/// <summary>
		/// Creates a new layout dimension with the specified pixel and percent offsets
		/// </summary>
		/// <param name="pixels">The raw pixel offset</param>
		/// <param name="percent">The percentage of the parent element's size to inherit</param>
		public LayoutUnit(float pixels = 0, float percent = 0) {
			ArgumentOutOfRangeException.ThrowIfLessThan(percent, 0, nameof(percent));
			ArgumentOutOfRangeException.ThrowIfGreaterThan(percent, 1, nameof(percent));

			this.pixels = pixels;
			this.percent = percent;
			_isDynamic = false;
		}

		// Used by DynamicSize only
		private LayoutUnit(float pixels, float percent, bool isDynamic) {
			this.pixels = pixels;
			this.percent = percent;
			_isDynamic = isDynamic;
		}

		/// <summary>
		/// Gets the raw value of the dimension with respect to the enclosing size
		/// </summary>
		public float GetValueRaw(float enclosingSize) => pixels + percent * enclosingSize;

		/// <summary>
		/// Whether this layout dimension is considered dynamic, i.e. the parent layout's size is dependent on the locations and sizes of its children
		/// </summary>
		public bool IsDynamic() => _isDynamic;

		/// <summary>
		/// Clamps the value of the dimension between the specified minimum and maximum values
		/// </summary>
		public static float Clamp(float value, LayoutUnit min, LayoutUnit max, float enclosingSize) {
			// If the enclosing parent hasn't been fully initialized, "enclosingSize" can end up being 0, which causes problems
			// Hence, a simple Math.Clamp() won't suffice here; it expects min <= max
			// Hence, we don't care if MinX is greater than MaxX, since MinX implies "this element must be at least this size"
			//  whereas MaxX is typically used to just restrict the size of the element to its parent's bounds

			float minRaw = min.GetValueRaw(enclosingSize);

			if (value < minRaw)
				return minRaw;

			float maxRaw = max.GetValueRaw(enclosingSize);

			if (value > maxRaw)
				return maxRaw;

			return value;
		}

		/// <summary>
		/// Converts the vanilla <see cref="StyleDimension"/> to a <see cref="LayoutUnit"/>
		/// </summary>
		public static implicit operator LayoutUnit(StyleDimension dimension) => new LayoutUnit(dimension.Pixels, dimension.Percent);

		/// <summary>
		/// Converts the <see cref="LayoutUnit"/> to a vanilla <see cref="StyleDimension"/>
		/// </summary>
		public static implicit operator StyleDimension(LayoutUnit unit) => new StyleDimension(unit.pixels, unit.percent);
	}
}
