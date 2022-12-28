using Terraria.ModLoader;

namespace SerousCommonLib.API {
	public abstract class Edit : ILoadable {
		public void Load(Mod mod) => LoadEdits();

		public abstract void LoadEdits();

		public void Unload() => UnloadEdits();

		public abstract void UnloadEdits();
	}
}
