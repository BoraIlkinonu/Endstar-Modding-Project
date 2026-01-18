using System;

namespace MatchmakingClientSDK.Diagnostics
{
	// Token: 0x0200006C RID: 108
	public class DiagnosticResult
	{
		// Token: 0x1700006F RID: 111
		// (get) Token: 0x0600040D RID: 1037 RVA: 0x000123A6 File Offset: 0x000105A6
		// (set) Token: 0x0600040E RID: 1038 RVA: 0x000123AE File Offset: 0x000105AE
		public string HostName { get; set; }

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x0600040F RID: 1039 RVA: 0x000123B7 File Offset: 0x000105B7
		// (set) Token: 0x06000410 RID: 1040 RVA: 0x000123BF File Offset: 0x000105BF
		public int Port { get; set; }

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000411 RID: 1041 RVA: 0x000123C8 File Offset: 0x000105C8
		// (set) Token: 0x06000412 RID: 1042 RVA: 0x000123D0 File Offset: 0x000105D0
		public double Duration { get; set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000413 RID: 1043 RVA: 0x000123D9 File Offset: 0x000105D9
		// (set) Token: 0x06000414 RID: 1044 RVA: 0x000123E1 File Offset: 0x000105E1
		public int BytesSent { get; set; }

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000415 RID: 1045 RVA: 0x000123EA File Offset: 0x000105EA
		// (set) Token: 0x06000416 RID: 1046 RVA: 0x000123F2 File Offset: 0x000105F2
		public int BytesReceived { get; set; }
	}
}
