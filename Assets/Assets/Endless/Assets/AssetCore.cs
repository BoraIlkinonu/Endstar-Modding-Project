using System;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x02000004 RID: 4
	[Serializable]
	public class AssetCore
	{
		// Token: 0x0600000C RID: 12 RVA: 0x0000210C File Offset: 0x0000030C
		public virtual AssetReference ToAssetReference()
		{
			return new AssetReference
			{
				AssetType = this.AssetType,
				AssetVersion = this.AssetVersion,
				AssetID = this.AssetID,
				UpdateParentVersion = false
			};
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002140 File Offset: 0x00000340
		public override string ToString()
		{
			return string.Concat(new string[] { "{ Name: ", this.Name, ", AssetID: ", this.AssetID, ", AssetVersion: ", this.AssetVersion, ", AssetType: ", this.AssetType, " }" });
		}

		// Token: 0x04000005 RID: 5
		[JsonProperty]
		public string Name;

		// Token: 0x04000006 RID: 6
		[JsonProperty("asset_id")]
		public string AssetID;

		// Token: 0x04000007 RID: 7
		[JsonProperty("asset_version")]
		public string AssetVersion;

		// Token: 0x04000008 RID: 8
		[JsonProperty("asset_type")]
		public string AssetType = "Unknown";
	}
}
