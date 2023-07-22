using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System;
using Terraria.ModLoader;
using Terraria.Localization;

namespace SerousCommonLib.API.Helpers
{
	/// <summary>
	/// A helper class for using localized text
	/// </summary>
	public static class LocalizationHelper
	{
		/// <summary>
		/// Forces the localization for the given mod, <paramref name="mod"/>, to be loaded for use with <seealso cref="Language"/>
		/// </summary>
		/// <param name="mod">The mod instance</param>
#if TML_2022_09
		internal static readonly MethodInfo LocalizationLoader_AutoloadTranslations =
			typeof(LocalizationLoader).GetMethod("AutoloadTranslations", BindingFlags.NonPublic | BindingFlags.Static);

		internal static readonly MethodInfo LocalizationLoader_SetLocalizedText =
			typeof(LocalizationLoader).GetMethod("SetLocalizedText", BindingFlags.NonPublic | BindingFlags.Static);

		internal static readonly FieldInfo LanguageManager__localizedTexts =
			typeof(LanguageManager).GetField("_localizedTexts", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void ForceLoadModHJsonLocalization(Mod mod) {
            Dictionary<string, ModTranslation> modTranslationDictionary = new();

            LocalizationLoader_AutoloadTranslations.Invoke(null, new object[] { mod, modTranslationDictionary });

            Dictionary<string, LocalizedText> dict =
 LanguageManager__localizedTexts.GetValue(LanguageManager.Instance) as Dictionary<string, LocalizedText>;

            var culture = Language.ActiveCulture;
            foreach (ModTranslation translation in modTranslationDictionary.Values) {
                //LocalizedText text = new LocalizedText(translation.Key, translation.GetTranslation(culture));
                LocalizedText text =
 Activator.CreateInstance(typeof(LocalizedText), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { translation.Key, translation.GetTranslation(culture) }, CultureInfo.InvariantCulture) as LocalizedText;

                LocalizationLoader_SetLocalizedText.Invoke(null, new object[] { dict, text });
            }
        }
#else
		private static readonly MethodInfo LocalizationLoader_LoadTranslations =
			typeof(LocalizationLoader).GetMethod("LoadTranslations", BindingFlags.NonPublic | BindingFlags.Static);

		private static readonly MethodInfo LocalizedText_SetValue =
			typeof(LocalizedText).GetMethod("SetValue", BindingFlags.NonPublic | BindingFlags.Instance);

		public static void ForceLoadModHJsonLocalization(Mod mod)
		{
			var lang = LanguageManager.Instance;
			foreach (var (key, value) in LocalizationLoader_LoadTranslations.Invoke(null, new object[] { mod, Language.ActiveCulture }) as List<(string key, string value)>)
			{
				var text = lang.GetText(key);
				LocalizedText_SetValue.Invoke(text,
					new object[] { value }); // can only set the value of existing keys. Cannot register new keys.
			}
		}
#endif
	}
}
