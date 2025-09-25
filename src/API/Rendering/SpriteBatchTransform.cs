using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SerousCommonLib.API.Rendering;

/// <summary>
/// An interface for modifying the transformation matrix of a <see cref="SpriteBatch"/>
/// </summary>
public class SpriteBatchTransform {
	/// <summary/>
	public readonly SpriteBatch spriteBatch;

	// 1-to-1 mapping of the fields in SpriteBatch
	
	/// <summary/>
	public readonly nint spriteMatrixTransform;
	/// <summary/>
	public readonly EffectPass spriteEffectPass;
	/// <summary/>
	public readonly Matrix transformMatrix;

	/// <summary>
	/// Captures the current transform-related parameters of a <see cref="SpriteBatch"/> instance.
	/// </summary>
	/// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance to capture the state of.</param>
	public SpriteBatchTransform(SpriteBatch spriteBatch) {
		this.spriteBatch = spriteBatch;
		spriteMatrixTransform = spriteBatch.spriteMatrixTransform;
		spriteEffectPass = spriteBatch.spriteEffectPass;
		transformMatrix = spriteBatch.transformMatrix;
	}

	/// <summary>
	/// Applies an adjustment matrix to the current transform matrix of the <see cref="SpriteBatch"/><br/>
	/// <b>NOTE:</b> Successive calls to this method will overwrite the previous adjustment.
	/// </summary>
	/// <param name="adjustmentMatrix"></param>
	public void ApplyTransform(Matrix adjustmentMatrix) {
		Matrix transform = transformMatrix * adjustmentMatrix;

		// Original: https://github.com/tModLoader/FNA/blob/2d16be6c66ffa9a157ff1bb50ffe215558b40394/src/Graphics/SpriteBatch.cs#L1423

		Viewport viewport = spriteBatch.GraphicsDevice.Viewport;

		// Inlined CreateOrthographicOffCenter * transformMatrix
		float tfWidth = (float) (2.0 / (double) viewport.Width);
		float tfHeight = (float) (-2.0 / (double) viewport.Height);
		unsafe
		{
			float* dstPtr = (float*) spriteMatrixTransform;
			dstPtr[0] = (tfWidth * transform.M11) - transform.M14;
			dstPtr[1] = (tfWidth * transform.M21) - transform.M24;
			dstPtr[2] = (tfWidth * transform.M31) - transform.M34;
			dstPtr[3] = (tfWidth * transform.M41) - transform.M44;
			dstPtr[4] = (tfHeight * transform.M12) + transform.M14;
			dstPtr[5] = (tfHeight * transform.M22) + transform.M24;
			dstPtr[6] = (tfHeight * transform.M32) + transform.M34;
			dstPtr[7] = (tfHeight * transform.M42) + transform.M44;
			dstPtr[8] = transform.M13;
			dstPtr[9] = transform.M23;
			dstPtr[10] = transform.M33;
			dstPtr[11] = transform.M43;
			dstPtr[12] = transform.M14;
			dstPtr[13] = transform.M24;
			dstPtr[14] = transform.M34;
			dstPtr[15] = transform.M44;
		}

		// FIXME: When is this actually applied? -flibit
		spriteEffectPass.Apply();
	}
}
