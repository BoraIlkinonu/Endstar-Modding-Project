using System;
using Endless.Assets;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.ParticleSystems.Assets
{
	// Token: 0x02000004 RID: 4
	[Serializable]
	public class ParticleSystemAsset : Asset
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600000E RID: 14 RVA: 0x000021CA File Offset: 0x000003CA
		[JsonIgnore]
		public FileAssetInstance BundleFileAsset
		{
			get
			{
				return this.WindowsStandaloneBundleFile;
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000021D2 File Offset: 0x000003D2
		public ParticleSystemAsset()
		{
			this.AssetType = "particle-system";
		}

		// Token: 0x04000005 RID: 5
		public static readonly SemanticVersion INTERNAL_VERSION = new SemanticVersion(1, 0, 0);

		// Token: 0x04000006 RID: 6
		[JsonProperty("prefab_file_name")]
		public string PrefabFileName = string.Empty;

		// Token: 0x04000007 RID: 7
		[JsonProperty("windows_standalone_bundle")]
		public FileAssetInstance WindowsStandaloneBundleFile;

		// Token: 0x04000008 RID: 8
		[JsonProperty("mac_standalone_bundle")]
		public FileAssetInstance MacStandaloneBundleFile;

		// Token: 0x04000009 RID: 9
		[JsonProperty("ios_bundle")]
		public FileAssetInstance IOSBundleFile;

		// Token: 0x0400000A RID: 10
		[JsonProperty("android_bundle")]
		public FileAssetInstance AndroidBundleFile;

		// Token: 0x0400000B RID: 11
		[JsonProperty("linux_bundle")]
		public FileAssetInstance LinuxBundleFile;

		// Token: 0x0400000C RID: 12
		[JsonProperty("unity_package_file")]
		public FileAssetInstance UnityPackageFile;

		// Token: 0x0400000D RID: 13
		[JsonProperty("icon_file_instance_id")]
		public int IconFileInstanceId;

		// Token: 0x0400000E RID: 14
		[JsonProperty("openSource")]
		public bool OpenSource;
	}
}
