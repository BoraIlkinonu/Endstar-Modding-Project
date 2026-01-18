using System;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x0200001A RID: 26
	public struct UserReportAttachment
	{
		// Token: 0x0600008B RID: 139 RVA: 0x0000443E File Offset: 0x0000263E
		public UserReportAttachment(string name, string fileName, string contentType, byte[] data)
		{
			this.Name = name;
			this.FileName = fileName;
			this.ContentType = contentType;
			this.DataBase64 = Convert.ToBase64String(data);
			this.DataIdentifier = null;
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600008C RID: 140 RVA: 0x00004469 File Offset: 0x00002669
		// (set) Token: 0x0600008D RID: 141 RVA: 0x00004471 File Offset: 0x00002671
		public string ContentType { readonly get; set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600008E RID: 142 RVA: 0x0000447A File Offset: 0x0000267A
		// (set) Token: 0x0600008F RID: 143 RVA: 0x00004482 File Offset: 0x00002682
		public string DataBase64 { readonly get; set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000090 RID: 144 RVA: 0x0000448B File Offset: 0x0000268B
		// (set) Token: 0x06000091 RID: 145 RVA: 0x00004493 File Offset: 0x00002693
		public string DataIdentifier { readonly get; set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000092 RID: 146 RVA: 0x0000449C File Offset: 0x0000269C
		// (set) Token: 0x06000093 RID: 147 RVA: 0x000044A4 File Offset: 0x000026A4
		public string FileName { readonly get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000094 RID: 148 RVA: 0x000044AD File Offset: 0x000026AD
		// (set) Token: 0x06000095 RID: 149 RVA: 0x000044B5 File Offset: 0x000026B5
		public string Name { readonly get; set; }
	}
}
