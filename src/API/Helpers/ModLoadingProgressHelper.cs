using System.Reflection;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SerousCommonLib.API.Helpers {
	/// <summary>
	/// A helper class for modifying the "sub-text" located under the loading bar during mod loading
	/// </summary>
	public static class ModLoadingProgressHelper {
		private static readonly FieldInfo Interface_loadMods = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface").GetField("loadMods", BindingFlags.NonPublic | BindingFlags.Static);
		private static readonly MethodInfo UIProgress_set_SubProgressText = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIProgress").GetProperty("SubProgressText", BindingFlags.Public | BindingFlags.Instance).GetSetMethod();

		/// <summary>
		/// Gets the localized string for "Finishing Resource Loading"
		/// </summary>
		public static string ProgressText_FinishResourceLoading => Language.GetTextValue("tModLoader.MSFinishingResourceLoading");

		/// <summary>
		/// Sets the loading "sub-text" to <paramref name="text"/>
		/// </summary>
		/// <param name="text">The text string to display</param>
		public static void SetLoadingSubProgressText(string text) => UIProgress_set_SubProgressText.Invoke(Interface_loadMods.GetValue(null), new object[] { text });
	}
}
