using System;

namespace Endless.Matchmaking
{
	// Token: 0x02000030 RID: 48
	public class PaginationParams
	{
		// Token: 0x06000127 RID: 295 RVA: 0x00005D33 File Offset: 0x00003F33
		public PaginationParams(string paginationQuery = "", int offset = 0, int limit = 50)
		{
			this.PaginationQuery = paginationQuery;
			this.Offset = offset;
			this.Limit = limit;
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00005D63 File Offset: 0x00003F63
		public void WithBaselineQuery()
		{
			this.PaginationQuery = "pagination { from to total }";
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00005D70 File Offset: 0x00003F70
		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}, {4}: {5}", new object[] { "Limit", this.Limit, "Offset", this.Offset, "PaginationQuery", this.PaginationQuery });
		}

		// Token: 0x04000077 RID: 119
		public int Limit = 50;

		// Token: 0x04000078 RID: 120
		public int Offset;

		// Token: 0x04000079 RID: 121
		public string PaginationQuery = string.Empty;
	}
}
