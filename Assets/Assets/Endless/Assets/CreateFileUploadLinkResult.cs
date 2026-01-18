using System;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x0200000F RID: 15
	[Serializable]
	public class CreateFileUploadLinkResult
	{
		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000038 RID: 56 RVA: 0x000025F7 File Offset: 0x000007F7
		// (set) Token: 0x06000039 RID: 57 RVA: 0x000025FF File Offset: 0x000007FF
		[JsonProperty("secure_upload_url")]
		public string SecureUploadURL { get; private set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600003A RID: 58 RVA: 0x00002608 File Offset: 0x00000808
		// (set) Token: 0x0600003B RID: 59 RVA: 0x00002610 File Offset: 0x00000810
		[JsonProperty("additional_s3_security_fields")]
		[JsonConverter(typeof(StringToAdditionalS3SecurityFieldsConverter))]
		public AdditionalS3SecurityFields AdditionalS3SecurityFields { get; private set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600003C RID: 60 RVA: 0x00002619 File Offset: 0x00000819
		// (set) Token: 0x0600003D RID: 61 RVA: 0x00002621 File Offset: 0x00000821
		[JsonProperty("file_id")]
		public int FileId { get; private set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600003E RID: 62 RVA: 0x0000262A File Offset: 0x0000082A
		// (set) Token: 0x0600003F RID: 63 RVA: 0x00002632 File Offset: 0x00000832
		[JsonProperty("file_instance_id")]
		public int FileInstanceId { get; private set; }
	}
}
