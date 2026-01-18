using System;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x02000020 RID: 32
	public struct UserReportNamedValue
	{
		// Token: 0x060000C1 RID: 193 RVA: 0x00004715 File Offset: 0x00002915
		public UserReportNamedValue(string name, string value)
		{
			this.Name = name;
			this.Value = value;
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060000C2 RID: 194 RVA: 0x00004725 File Offset: 0x00002925
		// (set) Token: 0x060000C3 RID: 195 RVA: 0x0000472D File Offset: 0x0000292D
		public string Name { readonly get; set; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000C4 RID: 196 RVA: 0x00004736 File Offset: 0x00002936
		// (set) Token: 0x060000C5 RID: 197 RVA: 0x0000473E File Offset: 0x0000293E
		public string Value { readonly get; set; }
	}
}
