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
		// Copied from: https://github.com/tModLoader/FNA/blob/2d16be6c66ffa9a157ff1bb50ffe215558b40394/src/Graphics/SpriteBatch.cs#L1423

		Viewport viewport = spriteBatch.GraphicsDevice.Viewport;

		// Inlined CreateOrthographicOffCenter * transformMatrix
		float tfWidth = (float) (2.0 / (double) viewport.Width);
		float tfHeight = (float) (-2.0 / (double) viewport.Height);
		unsafe
		{
			float* dstPtr = (float*) spriteMatrixTransform;
			dstPtr[0] = (tfWidth * transformMatrix.M11) - transformMatrix.M14;
			dstPtr[1] = (tfWidth * transformMatrix.M21) - transformMatrix.M24;
			dstPtr[2] = (tfWidth * transformMatrix.M31) - transformMatrix.M34;
			dstPtr[3] = (tfWidth * transformMatrix.M41) - transformMatrix.M44;
			dstPtr[4] = (tfHeight * transformMatrix.M12) + transformMatrix.M14;
			dstPtr[5] = (tfHeight * transformMatrix.M22) + transformMatrix.M24;
			dstPtr[6] = (tfHeight * transformMatrix.M32) + transformMatrix.M34;
			dstPtr[7] = (tfHeight * transformMatrix.M42) + transformMatrix.M44;
			dstPtr[8] = transformMatrix.M13;
			dstPtr[9] = transformMatrix.M23;
			dstPtr[10] = transformMatrix.M33;
			dstPtr[11] = transformMatrix.M43;
			dstPtr[12] = transformMatrix.M14;
			dstPtr[13] = transformMatrix.M24;
			dstPtr[14] = transformMatrix.M34;
			dstPtr[15] = transformMatrix.M44;
		}

		// FIXME: When is this actually applied? -flibit
		spriteEffectPass.Apply();
	}
}
