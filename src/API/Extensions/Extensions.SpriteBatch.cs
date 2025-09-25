using Microsoft.Xna.Framework.Graphics;

namespace SerousCommonLib.API;

partial class Extensions {
	public static void Begin(this SpriteBatch @this, SpriteBatchCapture capture) {
		@this.Begin(
			capture.sortMode,
			capture.blendState,
			capture.samplerState,
			capture.depthStencilState,
			capture.rasterizerState,
			capture.customEffect,
			capture.transformMatrix
		);
	}
	
	public static bool IsActive(this SpriteBatch @this) => @this.beginCalled;
}
