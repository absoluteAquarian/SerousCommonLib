using Microsoft.Xna.Framework;
using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An structure representing a calculated dimension of a layout
	/// </summary>
	public readonly struct LayoutDimension {
		/// <summary>
		/// The value of the dimension
		/// </summary>
		public readonly float value;

		/// <summary>
		/// Creates a new layout dimension with the specified value
		/// </summary>
		public LayoutDimension(float value) {
			this.value = value;
		}

		/// <summary/>
		public LayoutDimension Offset(float offset) => new LayoutDimension(value + offset);

		/// <summary/>
		public static LayoutDimension operator +(LayoutDimension a, LayoutDimension b) => new LayoutDimension(a.value + b.value);

		/// <summary/>
		public static LayoutDimension operator -(LayoutDimension a, LayoutDimension b) => new LayoutDimension(a.value - b.value);

		/// <summary/>
		public static implicit operator LayoutDimension(float value) => new LayoutDimension(value);

		/// <summary/>
		public static implicit operator float(LayoutDimension dimension) => dimension.value;
	}
}
