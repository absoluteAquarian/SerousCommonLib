using System;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A structure representing the bias for the constraints of a UI element
	/// </summary>
	public readonly struct ConstraintBias {
		/// <summary>
		/// If the UI element has constraints for both its Left and Right edges, this value determines how the total width is distributed between them.<br/>
		/// 0% = Left edge has no offset<br/>
		/// 50% = Left edge and Right edge are equally offset from the center<br/>
		/// 100% = Right edge has no offset
		/// </summary>
		public readonly float horizontalBias;

		/// <summary>
		/// If the UI element has constraints for both its Top and Bottom edges, this value determines how the total height is distributed between them.<br/>
		/// 0% = Top edge has no offset<br/>
		/// 50% = Top edge and Bottom edge are equally offset from the center<br/>
		/// 100% = Bottom edge has no offset
		/// </summary>
		public readonly float verticalBias;

		/// <summary>
		/// Creates a new instance of <see cref="ConstraintBias"/> with the provided horizontal and vertical bias
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"/>
		public ConstraintBias(float horizontalBias, float verticalBias) {
			ArgumentOutOfRangeException.ThrowIfLessThan(horizontalBias, 0, nameof(horizontalBias));
			ArgumentOutOfRangeException.ThrowIfGreaterThan(horizontalBias, 1, nameof(horizontalBias));
			ArgumentOutOfRangeException.ThrowIfLessThan(verticalBias, 0, nameof(verticalBias));
			ArgumentOutOfRangeException.ThrowIfGreaterThan(verticalBias, 1, nameof(verticalBias));

			this.horizontalBias = horizontalBias;
			this.verticalBias = verticalBias;
		}
	}
}
