using System;
using System.Collections.Generic;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x0200001E RID: 30
	public struct UserReportMeasure
	{
		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000AD RID: 173 RVA: 0x000045F7 File Offset: 0x000027F7
		// (set) Token: 0x060000AE RID: 174 RVA: 0x000045FF File Offset: 0x000027FF
		public int EndFrameNumber { readonly get; set; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060000AF RID: 175 RVA: 0x00004608 File Offset: 0x00002808
		// (set) Token: 0x060000B0 RID: 176 RVA: 0x00004610 File Offset: 0x00002810
		public List<UserReportNamedValue> Metadata { readonly get; set; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x00004619 File Offset: 0x00002819
		// (set) Token: 0x060000B2 RID: 178 RVA: 0x00004621 File Offset: 0x00002821
		public List<UserReportMetric> Metrics { readonly get; set; }

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x0000462A File Offset: 0x0000282A
		// (set) Token: 0x060000B4 RID: 180 RVA: 0x00004632 File Offset: 0x00002832
		public int StartFrameNumber { readonly get; set; }
	}
}
