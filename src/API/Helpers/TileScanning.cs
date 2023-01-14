using Microsoft.Xna.Framework;
using Terraria;

namespace SerousCommonLib.API.Helpers {
	/// <summary>
	/// A helper class for retrieving biome information around a given area
	/// </summary>
	public static class TileScanning {
		internal static bool DoBlockHooks;

		/// <summary>
		/// Performs biome scanning with <paramref name="worldCenter"/> as the center of the scanning area
		/// </summary>
		/// <param name="worldCenter">The world coordinates for the center of the scanning area</param>
		/// <returns>A <see cref="SceneMetrics"/> instance containing biome information, or <see langword="null"/> if an exception was caught</returns>
		public static SceneMetrics Scan(Vector2 worldCenter) {
			try {
				SceneMetrics sceneMetrics = new();

				SceneMetricsScanSettings settings = new SceneMetricsScanSettings {
					VisualScanArea = null,
					BiomeScanCenterPositionInWorld = worldCenter,
					ScanOreFinderData = false
				};

				DoBlockHooks = true;

				sceneMetrics.ScanAndExportToMain(settings);

				DoBlockHooks = false;

				// Reset the player's biomes to reset
				Main.LocalPlayer.ForceUpdateBiomes();

				return sceneMetrics;
			} catch {
				// Swallow any exceptions
				return null;
			}
		}
	}
}
