using System;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x02000013 RID: 19
	public class GameAssetPreview : Asset
	{
		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000048 RID: 72 RVA: 0x00002752 File Offset: 0x00000952
		[JsonIgnore]
		public static string AssetReturnArgs
		{
			get
			{
				return "{ asset_id asset_version asset_type iconFileInstanceId Name Description internal_version revision_meta_data { revision_timestamp changes } screenshot { asset_file_instance_id } }";
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000049 RID: 73 RVA: 0x00002759 File Offset: 0x00000959
		[JsonIgnore]
		public int IconFileInstanceId
		{
			get
			{
				if (this.screenshot != null)
				{
					return this.screenshot.AssetFileInstanceId;
				}
				return this.iconFileInstanceId;
			}
		}

		// Token: 0x04000063 RID: 99
		[JsonProperty("screenshot")]
		private FileAssetInstance screenshot;

		// Token: 0x04000064 RID: 100
		[JsonProperty("iconFileInstanceId")]
		private int iconFileInstanceId;
	}
}
