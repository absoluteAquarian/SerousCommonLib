using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace SerousCommonLib.API.Input {
	/// <summary>
	/// A helper class for legacy mouse input, i.e. not part of the <see cref="UIElement"/> event system
	/// </summary>
	public static class LegacyMouseInput {
		/// <summary>
		/// Returns whether the left mouse button was clicked this frame
		/// </summary>
		public static bool MouseClicked => MouseActionsWatchdog.MouseClicked;

		/// <summary>
		/// Returns whether the right mouse button was clicked this frame
		/// </summary>
		public static bool RightMouseClicked => MouseActionsWatchdog.RightMouseClicked;
	}

	internal class MouseActionsWatchdog : ModSystem {
		private static GameTime lastGameTime;

		public static MouseState curMouse;
		public static MouseState oldMouse;

		public static bool MouseClicked => curMouse.LeftButton == ButtonState.Pressed && oldMouse.LeftButton == ButtonState.Released;
		public static bool RightMouseClicked => curMouse.RightButton == ButtonState.Pressed && oldMouse.RightButton == ButtonState.Released;

		public override void UpdateUI(GameTime gameTime) {
			lastGameTime = gameTime;

			TextInputTracker.RestrictUpdates();
			oldMouse = curMouse;
			curMouse = Mouse.GetState();
		}

		public override void PostUpdateInput() {
			TextInputTracker.RequestFullUpdates();

			if (Main.dedServ)
				return;

			TextInputTracker.Update(lastGameTime);

			TextInputTracker.RestrictUpdates();
		}
	}
}
