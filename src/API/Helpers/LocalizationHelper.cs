using Terraria.ModLoader;
using Terraria.Localization;

namespace SerousCommonLib.API.Helpers {
	/// <summary>
	/// A helper class for using localized text
	/// </summary>
	public static class LocalizationHelper {
		/// <summary>
		/// Forces the localization for the given mod, <paramref name="mod"/>, to be loaded for use with <seealso cref="Language"/>
		/// </summary>
		/// <param name="mod">The mod instance</param>
		public static void ForceLoadModHJsonLocalization(Mod mod) {
#if TML_2022_09
			Dictionary<string, ModTranslation> modTranslationDictionary = new();

			LocalizationLoader.AutoloadTranslations(mod, modTranslationDictionary);

			Dictionary<string, LocalizedText> dict = LanguageManager.Instance._localizedTexts;

			var culture = Language.ActiveCulture;
			foreach (ModTranslation translation in modTranslationDictionary.Values) {
				LocalizedText text = new LocalizedText(translation.Key, translation.GetTranslation(culture));
				LocalizationLoader.SetLocalizedText(dict, text);
			}
#else
			var lang = LanguageManager.Instance;
			foreach (var (key, value) in LocalizationLoader.LoadTranslations(mod, Language.ActiveCulture)) {
				var text = lang.GetText(key);
				text.SetValue(value); // can only set the value of existing keys. Cannot register new keys.
			}
#endif
		}
	}
}
