using System;
using Endless.Assets;
using Newtonsoft.Json;

namespace Endless.TerrainCosmetics
{
	// Token: 0x0200000C RID: 12
	[Serializable]
	public class TerrainTilesetCosmeticAsset : Asset
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000030 RID: 48 RVA: 0x00002509 File Offset: 0x00000709
		[JsonIgnore]
		public FileAssetInstance BundleFileAsset
		{
			get
			{
				return this.WindowsStandaloneBundleFile;
			}
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002511 File Offset: 0x00000711
		public TerrainTilesetCosmeticAsset()
		{
			this.AssetType = "terrain-tileset-cosmetic";
		}

		// Token: 0x0400001B RID: 27
		[JsonProperty("inherited_tileset_id")]
		public string InheritedTilesetId;

		// Token: 0x0400001C RID: 28
		[JsonProperty("open_source")]
		public bool OpenSource;

		// Token: 0x0400001D RID: 29
		[JsonProperty("screenshot")]
		public FileAssetInstance DisplayIconFileInstance;

		// Token: 0x0400001E RID: 30
		[JsonProperty("windows_standalone_bundle")]
		public FileAssetInstance WindowsStandaloneBundleFile;

		// Token: 0x0400001F RID: 31
		[JsonProperty("mac_standalone_bundle")]
		public FileAssetInstance MacStandaloneBundleFile;

		// Token: 0x04000020 RID: 32
		[JsonProperty("ios_bundle")]
		public FileAssetInstance IOSBundleFile;

		// Token: 0x04000021 RID: 33
		[JsonProperty("android_bundle")]
		public FileAssetInstance AndroidBundleFile;

		// Token: 0x04000022 RID: 34
		[JsonProperty("linux_bundle")]
		public FileAssetInstance LinuxBundleFile;

		// Token: 0x04000023 RID: 35
		[JsonProperty("unity_package_file")]
		public FileAssetInstance UnityPackageFile;
	}
}
