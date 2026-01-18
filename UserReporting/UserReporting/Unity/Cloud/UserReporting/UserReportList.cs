using System;
using System.Collections.Generic;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x0200001D RID: 29
	public class UserReportList
	{
		// Token: 0x060000A3 RID: 163 RVA: 0x00004541 File Offset: 0x00002741
		public UserReportList()
		{
			this.UserReportPreviews = new List<UserReportPreview>();
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000A4 RID: 164 RVA: 0x00004554 File Offset: 0x00002754
		// (set) Token: 0x060000A5 RID: 165 RVA: 0x0000455C File Offset: 0x0000275C
		public string ContinuationToken { get; set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000A6 RID: 166 RVA: 0x00004565 File Offset: 0x00002765
		// (set) Token: 0x060000A7 RID: 167 RVA: 0x0000456D File Offset: 0x0000276D
		public string Error { get; set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000A8 RID: 168 RVA: 0x00004576 File Offset: 0x00002776
		// (set) Token: 0x060000A9 RID: 169 RVA: 0x0000457E File Offset: 0x0000277E
		public bool HasMore { get; set; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000AA RID: 170 RVA: 0x00004587 File Offset: 0x00002787
		// (set) Token: 0x060000AB RID: 171 RVA: 0x0000458F File Offset: 0x0000278F
		public List<UserReportPreview> UserReportPreviews { get; set; }

		// Token: 0x060000AC RID: 172 RVA: 0x00004598 File Offset: 0x00002798
		public void Complete(int originalLimit, string continuationToken)
		{
			if (this.UserReportPreviews.Count > 0 && this.UserReportPreviews.Count > originalLimit)
			{
				while (this.UserReportPreviews.Count > originalLimit)
				{
					this.UserReportPreviews.RemoveAt(this.UserReportPreviews.Count - 1);
				}
				this.ContinuationToken = continuationToken;
				this.HasMore = true;
			}
		}
	}
}
