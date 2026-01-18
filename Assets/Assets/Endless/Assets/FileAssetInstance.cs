using System;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x02000011 RID: 17
	[Serializable]
	public class FileAssetInstance
	{
		// Token: 0x04000060 RID: 96
		[JsonProperty("label")]
		public string Label;

		// Token: 0x04000061 RID: 97
		[JsonProperty("asset_file_instance_id")]
		public int AssetFileInstanceId;
	}
}
