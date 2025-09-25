using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SerousCommonLib.API.Rendering;

/// <summary>
/// An object representing a capture of the parameters for a <see cref="SpriteBatch"/> instance.
/// </summary>
public class SpriteBatchCapture {
	/// <summary/>
	public readonly SpriteBatch spriteBatch;

	// 1-to-1 mapping to the fields affected by SpriteBatch.Begin()

	/// <summary/>
	public readonly SpriteSortMode sortMode;
	/// <summary/>
	public readonly BlendState blendState;
	/// <summary/>
	public readonly SamplerState samplerState;
	/// <summary/>
	public readonly DepthStencilState depthStencilState;
	/// <summary/>
	public readonly RasterizerState rasterizerState;
	/// <summary/>
	public readonly Effect customEffect;
	/// <summary/>
	public readonly Matrix transformMatrix;

	/// <summary>
	/// Captures the current state of a <see cref="SpriteBatch"/> instance.
	/// </summary>
	/// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance to capture the state of.</param>
	public SpriteBatchCapture(SpriteBatch spriteBatch) {
		this.spriteBatch = spriteBatch;
		sortMode = spriteBatch.sortMode;
		blendState = spriteBatch.blendState;
		samplerState = spriteBatch.samplerState;
		depthStencilState = spriteBatch.depthStencilState;
		rasterizerState = spriteBatch.rasterizerState;
		customEffect = spriteBatch.customEffect;
		transformMatrix = spriteBatch.transformMatrix;
	}
}
