using System;
using Endless.Assets;
using Newtonsoft.Json;

namespace Endless.Props.Assets
{
	// Token: 0x02000039 RID: 57
	[Serializable]
	public class EndlessPrefabAsset : Asset
	{
		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060000E6 RID: 230 RVA: 0x000033A0 File Offset: 0x000015A0
		[JsonIgnore]
		public FileAssetInstance BundleFileAsset
		{
			get
			{
				return this.WindowsStandaloneBundleFile;
			}
		}

		// Token: 0x04000095 RID: 149
		[JsonProperty("prefab_file_name")]
		public string PrefabFileName = string.Empty;

		// Token: 0x04000096 RID: 150
		[JsonProperty("windows_standalone_bundle")]
		public FileAssetInstance WindowsStandaloneBundleFile;

		// Token: 0x04000097 RID: 151
		[JsonProperty("mac_standalone_bundle")]
		public FileAssetInstance MacStandaloneBundleFile;

		// Token: 0x04000098 RID: 152
		[JsonProperty("ios_bundle")]
		public FileAssetInstance IOSBundleFile;

		// Token: 0x04000099 RID: 153
		[JsonProperty("android_bundle")]
		public FileAssetInstance AndroidBundleFile;

		// Token: 0x0400009A RID: 154
		[JsonProperty("linux_bundle")]
		public FileAssetInstance LinuxBundleFile;

		// Token: 0x0400009B RID: 155
		[JsonProperty("unity_package_file")]
		public FileAssetInstance UnityPackageFile;
	}
}
