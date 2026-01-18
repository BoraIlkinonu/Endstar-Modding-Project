using System;
using Endless.Assets;
using Newtonsoft.Json;

namespace Runtime.Gameplay.LevelEditing
{
	// Token: 0x02000024 RID: 36
	[Serializable]
	public class PropUsage
	{
		// Token: 0x06000087 RID: 135 RVA: 0x00003A80 File Offset: 0x00001C80
		public static implicit operator AssetReference(PropUsage propUsage)
		{
			return new AssetReference
			{
				AssetID = propUsage.AssetID,
				AssetType = propUsage.AssetType,
				AssetVersion = propUsage.AssetVersion,
				UpdateParentVersion = false
			};
		}

		// Token: 0x04000052 RID: 82
		[JsonProperty("asset_ref_id")]
		public string AssetID;

		// Token: 0x04000053 RID: 83
		[JsonProperty("asset_ref_version")]
		public string AssetVersion;

		// Token: 0x04000054 RID: 84
		[JsonProperty("asset_type")]
		public string AssetType = "Prop";
	}
}
