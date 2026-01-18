using System;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x02000007 RID: 7
	[Serializable]
	public class AssetReference
	{
		// Token: 0x06000014 RID: 20 RVA: 0x00002278 File Offset: 0x00000478
		public override bool Equals(object obj)
		{
			AssetReference assetReference = obj as AssetReference;
			return assetReference != null && this.AssetID == assetReference.AssetID && this.AssetVersion == assetReference.AssetVersion && this.AssetType == assetReference.AssetType;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000022C8 File Offset: 0x000004C8
		public override int GetHashCode()
		{
			return HashCode.Combine<string, string, string>(this.AssetID, this.AssetVersion, this.AssetType);
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000022E1 File Offset: 0x000004E1
		public static bool operator ==(AssetReference a, AssetReference b)
		{
			return a == b || (a != null && b != null && a.Equals(b));
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000022FA File Offset: 0x000004FA
		public static bool operator !=(AssetReference a, AssetReference b)
		{
			return !(a == b);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002306 File Offset: 0x00000506
		public override string ToString()
		{
			return string.Concat(new string[] { this.AssetID, " (", this.AssetVersion, ") - ", this.AssetType });
		}

		// Token: 0x0400000E RID: 14
		[JsonProperty("asset_ref_id")]
		public string AssetID;

		// Token: 0x0400000F RID: 15
		[JsonProperty("asset_ref_version")]
		public string AssetVersion;

		// Token: 0x04000010 RID: 16
		[JsonProperty("asset_type")]
		public string AssetType = "Unknown";

		// Token: 0x04000011 RID: 17
		[JsonProperty("asset_need_update_parent_version")]
		public bool UpdateParentVersion;
	}
}
