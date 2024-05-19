using Terraria.Localization;
using Terraria.ModLoader.UI;

namespace SerousCommonLib.API.Helpers {
	/// <summary>
	/// A helper class for modifying the "sub-text" located under the loading bar during mod loading
	/// </summary>
	public static class ModLoadingProgressHelper {
		/// <summary>
		/// Gets the localized string for "Finishing Resource Loading"
		/// </summary>
		public static string ProgressText_FinishResourceLoading => Language.GetTextValue("tModLoader.MSFinishingResourceLoading");

		/// <summary>
		/// Sets the loading "sub-text" to <paramref name="text"/>
		/// </summary>
		/// <param name="text">The text string to display</param>
		public static void SetLoadingSubProgressText(string text) => Interface.loadMods.SubProgressText = text;
	}
}
