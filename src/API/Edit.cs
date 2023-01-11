using Terraria.ModLoader;

namespace SerousCommonLib.API {
	/// <summary>
	/// A simple class for loading and unloading edits
	/// </summary>
	public abstract class Edit : ILoadable {
		#pragma warning disable CS1591
		public void Load(Mod mod) => LoadEdits();

		public abstract void LoadEdits();

		public void Unload() => UnloadEdits();

		public abstract void UnloadEdits();
	}
}
