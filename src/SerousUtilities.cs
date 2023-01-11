using SerousCommonLib.API.Helpers;
using Terraria.ModLoader;

namespace SerousCommonLib {
	#pragma warning disable CS1591
	public class SerousUtilities : Mod {
		public override void Load() {
			LocalizationHelper.ForceLoadModHJsonLocalization(this);
		}
	}
}