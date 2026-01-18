using System;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x02000022 RID: 34
	public struct UserReportScreenshot
	{
		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060000E7 RID: 231 RVA: 0x0000486A File Offset: 0x00002A6A
		// (set) Token: 0x060000E8 RID: 232 RVA: 0x00004872 File Offset: 0x00002A72
		public string DataBase64 { readonly get; set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060000E9 RID: 233 RVA: 0x0000487B File Offset: 0x00002A7B
		// (set) Token: 0x060000EA RID: 234 RVA: 0x00004883 File Offset: 0x00002A83
		public string DataIdentifier { readonly get; set; }

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060000EB RID: 235 RVA: 0x0000488C File Offset: 0x00002A8C
		// (set) Token: 0x060000EC RID: 236 RVA: 0x00004894 File Offset: 0x00002A94
		public int FrameNumber { readonly get; set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060000ED RID: 237 RVA: 0x0000489D File Offset: 0x00002A9D
		public int Height
		{
			get
			{
				return PngHelper.GetPngHeightFromBase64Data(this.DataBase64);
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060000EE RID: 238 RVA: 0x000048AA File Offset: 0x00002AAA
		public int Width
		{
			get
			{
				return PngHelper.GetPngWidthFromBase64Data(this.DataBase64);
			}
		}
	}
}
