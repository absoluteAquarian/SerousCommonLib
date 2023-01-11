using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace SerousCommonLib.UI {
	/// <summary>
	/// A shorthand for <see cref="UITextPanel{T}"/>
	/// </summary>
	public class UIPanelTab : UITextPanel<LocalizedText> {
		#pragma warning disable CS1591
		public readonly string Name;

		public UIPanelTab(string name, LocalizedText text, float textScale = 1, bool large = false) : base(text, textScale, large) {
			Name = name;
		}
	}
}
