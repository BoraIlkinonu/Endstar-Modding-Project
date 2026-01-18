using System;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x0200000D RID: 13
	[Serializable]
	public class AdditionalS3SecurityFields
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000023 RID: 35 RVA: 0x000024E7 File Offset: 0x000006E7
		// (set) Token: 0x06000024 RID: 36 RVA: 0x000024EF File Offset: 0x000006EF
		[JsonProperty("Content-Type")]
		public string ContentType { get; private set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000025 RID: 37 RVA: 0x000024F8 File Offset: 0x000006F8
		// (set) Token: 0x06000026 RID: 38 RVA: 0x00002500 File Offset: 0x00000700
		[JsonProperty("bucket")]
		public string Bucket { get; private set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000027 RID: 39 RVA: 0x00002509 File Offset: 0x00000709
		// (set) Token: 0x06000028 RID: 40 RVA: 0x00002511 File Offset: 0x00000711
		[JsonProperty("X-Amz-Algorithm")]
		public string XAmzAlgorithm { get; private set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000029 RID: 41 RVA: 0x0000251A File Offset: 0x0000071A
		// (set) Token: 0x0600002A RID: 42 RVA: 0x00002522 File Offset: 0x00000722
		[JsonProperty("X-Amz-Credential")]
		public string XAmzCredential { get; private set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600002B RID: 43 RVA: 0x0000252B File Offset: 0x0000072B
		// (set) Token: 0x0600002C RID: 44 RVA: 0x00002533 File Offset: 0x00000733
		[JsonProperty("X-Amz-Date")]
		public string XAmzDate { get; private set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600002D RID: 45 RVA: 0x0000253C File Offset: 0x0000073C
		// (set) Token: 0x0600002E RID: 46 RVA: 0x00002544 File Offset: 0x00000744
		[JsonProperty("key")]
		public string Key { get; private set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600002F RID: 47 RVA: 0x0000254D File Offset: 0x0000074D
		// (set) Token: 0x06000030 RID: 48 RVA: 0x00002555 File Offset: 0x00000755
		[JsonProperty("Policy")]
		public string Policy { get; private set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000031 RID: 49 RVA: 0x0000255E File Offset: 0x0000075E
		// (set) Token: 0x06000032 RID: 50 RVA: 0x00002566 File Offset: 0x00000766
		[JsonProperty("X-Amz-Signature")]
		public string XAmzSignature { get; private set; }
	}
}
