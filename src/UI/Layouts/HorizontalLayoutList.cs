using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A variant of <see cref="HorizontalLayout"/> that clips the rendering of its elements to its bounds
	/// </summary>
	[Obsolete("This class is part of a new API that is not yet complete.", error: true)]
	public class HorizontalLayoutList : BaseOrderedLayoutList {
		private class InnerLayout : HorizontalLayout {
			public override bool ContainsPoint(Vector2 point) => true;

			protected override void DrawChildren(SpriteBatch spriteBatch) => RenderClippedElement(this, spriteBatch);

			public override Rectangle GetViewCullingArea() => Parent.GetDimensions().ToRectangle();
		}

		/// <inheritdoc/>
		public override ElementAlignmentMode AlignmentMode => ElementAlignmentMode.Horizontal;

		/// <inheritdoc/>
		protected override BaseOrderedLayout CreateInnerLayout() => new InnerLayout();
	}
}
