using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SerousCommonLib.API;

/// <summary>
/// An object representing a capture of the parameters for a <see cref="SpriteBatch"/> instance.
/// </summary>
public class SpriteBatchCapture {
	// 1-to-1 mapping to the fields affected by SpriteBatch.Begin()

	public readonly SpriteSortMode sortMode;
	public readonly BlendState blendState;
	public readonly SamplerState samplerState;
	public readonly DepthStencilState depthStencilState;
	public readonly RasterizerState rasterizerState;
	public readonly Effect customEffect;
	public readonly Matrix transformMatrix;

	public SpriteBatchCapture(SpriteBatch spriteBatch) {
		sortMode = spriteBatch.sortMode;
		blendState = spriteBatch.blendState;
		samplerState = spriteBatch.samplerState;
		depthStencilState = spriteBatch.depthStencilState;
		rasterizerState = spriteBatch.rasterizerState;
		customEffect = spriteBatch.customEffect;
		transformMatrix = spriteBatch.transformMatrix;
	}
}
