using SerousCommonLib.API;
using SerousCommonLib.API.Helpers;
using Terraria.ModLoader;

namespace SerousCommonLib {
	#pragma warning disable CS1591
	public class SerousUtilities : Mod {
		public static SerousUtilities Instance => ModContent.GetInstance<SerousUtilities>();

		public override void Load() {
			LocalizationHelper.ForceLoadModHJsonLocalization(this);
		}

		public override void Unload() {
			ILHelper.ClearPatchSources();
		}
	}
}