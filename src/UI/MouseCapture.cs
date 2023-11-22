using System;
using Terraria;

namespace SerousCommonLib.UI {
	internal readonly struct MouseCapture : IDisposable {
		public readonly bool left, leftReleased, right, rightRelease;

		private MouseCapture(bool left, bool leftReleased, bool right, bool rightRelease) {
			this.left = left;
			this.leftReleased = leftReleased;
			this.right = right;
			this.rightRelease = rightRelease;
		}

		public static MouseCapture CaptureAndReset() {
			var capture = new MouseCapture(Main.mouseLeft, Main.mouseLeftRelease, Main.mouseRight, Main.mouseRightRelease);

			Main.mouseLeft = false;
			Main.mouseLeftRelease = false;
			Main.mouseRight = false;
			Main.mouseRightRelease = false;

			return capture;
		}

		public void Dispose() {
			Main.mouseLeft = left;
			Main.mouseLeftRelease = leftReleased;
			Main.mouseRight = right;
			Main.mouseRightRelease = rightRelease;
		}
	}
}
