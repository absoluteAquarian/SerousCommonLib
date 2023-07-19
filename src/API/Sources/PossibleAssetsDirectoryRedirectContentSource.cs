using ReLogic.Content.Sources;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System;
using Terraria.ModLoader.Core;
using Terraria;
using System.Linq;
using Terraria.ModLoader.Assets;
using Terraria.ModLoader;
using System.Reflection;

namespace SerousCommonLib.API.Sources {
	// Near-copy of Terraria.ModLoader.Assets.TModContentSource
	/// <summary>
	/// A clone of <see cref="TModContentSource"/> that also checks for an "Assets/" folder.
	/// <para>
	/// As an example, the file <c>"TheMod/Assets/Folder/Thing.png"</c> can be read by using <c>"TheMod/Assets/Folder/Thing"</c> OR <c>"TheMod/Folder/Thing"</c>
	/// </para>
	/// </summary>
	public class PossibleAssetsDirectoryRedirectContentSource : ContentSource {
		private readonly TmodFile file;

		private static readonly MethodInfo Mod_get_File = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod();

		/// <summary>
		/// Creates an instance of a <see cref="PossibleAssetsDirectoryRedirectContentSource"/> from a mod instance
		/// </summary>
		/// <param name="mod">The mod instance</param>
		public PossibleAssetsDirectoryRedirectContentSource(Mod mod) : this(Mod_get_File.Invoke(mod, null) as TmodFile) { }

		/// <summary>
		/// Creates an instance of a <see cref="PossibleAssetsDirectoryRedirectContentSource"/> from a mod's file
		/// </summary>
		/// <param name="file">The Mod.File object</param>
		public PossibleAssetsDirectoryRedirectContentSource(TmodFile file) {
			ArgumentNullException.ThrowIfNull(file);

			this.file = file;

			// Skip loading assets on servers
			if (Main.dedServ)
				return;

			// From Terraria.Initializers.AssetInitializer::CreateAssetServices()
			//     services.AddService(typeof(AssetReaderCollection), assetReaderCollection);
			var assetReaderCollection = Main.instance.Services.GetService(typeof(AssetReaderCollection)) as AssetReaderCollection;

			var files = file.Select(static fileEntry => fileEntry.Name);
			var filesWithoutAssetsDirectory = file.Where(static fileEntry => fileEntry.Name.StartsWith("Assets/"))
				.Select(static fileEntry => fileEntry.Name[7..]);

			SetAssetNames(files.Concat(filesWithoutAssetsDirectory)
				.Where(name => assetReaderCollection.TryGetReader(Path.GetExtension(name), out _)));
		}

		/// <inheritdoc cref="TModContentSource.OpenStream(string)"/>
		public override Stream OpenStream(string fullAssetName) {
			// File exists without a redirection.  Attempt to use it
			if (file.HasFile(fullAssetName))
				return file.GetStream(fullAssetName, newFileStream: true);

			// File might need a redirection...
			if (!fullAssetName.StartsWith("Assets/") && file.HasFile("Assets/" + fullAssetName))
				return file.GetStream("Assets/" + fullAssetName, newFileStream: true);

			// File not found
			throw new KeyNotFoundException(fullAssetName);
		}
	}
}
