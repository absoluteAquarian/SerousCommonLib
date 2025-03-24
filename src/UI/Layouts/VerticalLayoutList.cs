using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A variant of <see cref="VerticalLayout"/> that clips the rendering of its elements to its bounds
	/// </summary>
	[Obsolete("This class is part of a new API that is not yet complete.", error: true)]
	public class VerticalLayoutList : BaseOrderedLayoutList {
		private class InnerLayout : VerticalLayout {
			public override bool ContainsPoint(Vector2 point) => true;

			protected override void DrawChildren(SpriteBatch spriteBatch) => RenderClippedElement(this, spriteBatch);

			public override Rectangle GetViewCullingArea() => Parent.GetDimensions().ToRectangle();
		}

		/// <inheritdoc/>
		protected override BaseOrderedLayout CreateInnerLayout() => new InnerLayout();
	}
}
