using System;
using Newtonsoft.Json;

namespace Endless.Shared.DataTypes
{
	// Token: 0x02000011 RID: 17
	public class PublishedVersion
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000063 RID: 99 RVA: 0x000034FA File Offset: 0x000016FA
		// (set) Token: 0x06000064 RID: 100 RVA: 0x00003502 File Offset: 0x00001702
		[JsonProperty("asset_version")]
		public string AssetVersion { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000065 RID: 101 RVA: 0x0000350B File Offset: 0x0000170B
		// (set) Token: 0x06000066 RID: 102 RVA: 0x00003513 File Offset: 0x00001713
		[JsonProperty("state")]
		public string State { get; set; }

		// Token: 0x06000067 RID: 103 RVA: 0x0000351C File Offset: 0x0000171C
		public override string ToString()
		{
			return "AssetVersion: " + this.AssetVersion + ", State: " + this.State;
		}
	}
}
