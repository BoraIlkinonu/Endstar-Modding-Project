using System;
using System.Collections.Generic;
using Unity.Cloud.Authorization;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x02000021 RID: 33
	public class UserReportPreview
	{
		// Token: 0x060000C6 RID: 198 RVA: 0x00004747 File Offset: 0x00002947
		public UserReportPreview()
		{
			this.Dimensions = new List<UserReportNamedValue>();
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x0000475A File Offset: 0x0000295A
		// (set) Token: 0x060000C8 RID: 200 RVA: 0x00004762 File Offset: 0x00002962
		public List<UserReportMetric> AggregateMetrics { get; set; }

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000C9 RID: 201 RVA: 0x0000476B File Offset: 0x0000296B
		// (set) Token: 0x060000CA RID: 202 RVA: 0x00004773 File Offset: 0x00002973
		public UserReportAppearanceHint AppearanceHint { get; set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000CB RID: 203 RVA: 0x0000477C File Offset: 0x0000297C
		// (set) Token: 0x060000CC RID: 204 RVA: 0x00004784 File Offset: 0x00002984
		public long ContentLength { get; set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000CD RID: 205 RVA: 0x0000478D File Offset: 0x0000298D
		// (set) Token: 0x060000CE RID: 206 RVA: 0x00004795 File Offset: 0x00002995
		public List<UserReportNamedValue> Dimensions { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000CF RID: 207 RVA: 0x0000479E File Offset: 0x0000299E
		// (set) Token: 0x060000D0 RID: 208 RVA: 0x000047A6 File Offset: 0x000029A6
		public DateTime ExpiresOn { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000D1 RID: 209 RVA: 0x000047AF File Offset: 0x000029AF
		// (set) Token: 0x060000D2 RID: 210 RVA: 0x000047B7 File Offset: 0x000029B7
		public string GeoCountry { get; set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000D3 RID: 211 RVA: 0x000047C0 File Offset: 0x000029C0
		// (set) Token: 0x060000D4 RID: 212 RVA: 0x000047C8 File Offset: 0x000029C8
		public string Identifier { get; set; }

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000D5 RID: 213 RVA: 0x000047D1 File Offset: 0x000029D1
		// (set) Token: 0x060000D6 RID: 214 RVA: 0x000047D9 File Offset: 0x000029D9
		public string IPAddress { get; set; }

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000D7 RID: 215 RVA: 0x000047E2 File Offset: 0x000029E2
		// (set) Token: 0x060000D8 RID: 216 RVA: 0x000047EA File Offset: 0x000029EA
		public bool IsHiddenWithoutDimension { get; set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000D9 RID: 217 RVA: 0x000047F3 File Offset: 0x000029F3
		// (set) Token: 0x060000DA RID: 218 RVA: 0x000047FB File Offset: 0x000029FB
		public bool IsSilent { get; set; }

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000DB RID: 219 RVA: 0x00004804 File Offset: 0x00002A04
		// (set) Token: 0x060000DC RID: 220 RVA: 0x0000480C File Offset: 0x00002A0C
		public bool IsTemporary { get; set; }

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000DD RID: 221 RVA: 0x00004815 File Offset: 0x00002A15
		// (set) Token: 0x060000DE RID: 222 RVA: 0x0000481D File Offset: 0x00002A1D
		public LicenseLevel LicenseLevel { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000DF RID: 223 RVA: 0x00004826 File Offset: 0x00002A26
		// (set) Token: 0x060000E0 RID: 224 RVA: 0x0000482E File Offset: 0x00002A2E
		public string ProjectIdentifier { get; set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000E1 RID: 225 RVA: 0x00004837 File Offset: 0x00002A37
		// (set) Token: 0x060000E2 RID: 226 RVA: 0x0000483F File Offset: 0x00002A3F
		public DateTime ReceivedOn { get; set; }

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000E3 RID: 227 RVA: 0x00004848 File Offset: 0x00002A48
		// (set) Token: 0x060000E4 RID: 228 RVA: 0x00004850 File Offset: 0x00002A50
		public string Summary { get; set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x00004859 File Offset: 0x00002A59
		// (set) Token: 0x060000E6 RID: 230 RVA: 0x00004861 File Offset: 0x00002A61
		public UserReportScreenshot Thumbnail { get; set; }
	}
}
