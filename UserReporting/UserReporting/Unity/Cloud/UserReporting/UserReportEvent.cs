using System;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x0200001B RID: 27
	public struct UserReportEvent
	{
		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000096 RID: 150 RVA: 0x000044BE File Offset: 0x000026BE
		// (set) Token: 0x06000097 RID: 151 RVA: 0x000044C6 File Offset: 0x000026C6
		public SerializableException Exception { readonly get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000098 RID: 152 RVA: 0x000044CF File Offset: 0x000026CF
		// (set) Token: 0x06000099 RID: 153 RVA: 0x000044D7 File Offset: 0x000026D7
		public int FrameNumber { readonly get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600009A RID: 154 RVA: 0x000044E0 File Offset: 0x000026E0
		public string FullMessage
		{
			get
			{
				return string.Format("{0}{1}{2}", this.Message, Environment.NewLine, this.StackTrace);
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600009B RID: 155 RVA: 0x000044FD File Offset: 0x000026FD
		// (set) Token: 0x0600009C RID: 156 RVA: 0x00004505 File Offset: 0x00002705
		public UserReportEventLevel Level { readonly get; set; }

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600009D RID: 157 RVA: 0x0000450E File Offset: 0x0000270E
		// (set) Token: 0x0600009E RID: 158 RVA: 0x00004516 File Offset: 0x00002716
		public string Message { readonly get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600009F RID: 159 RVA: 0x0000451F File Offset: 0x0000271F
		// (set) Token: 0x060000A0 RID: 160 RVA: 0x00004527 File Offset: 0x00002727
		public string StackTrace { readonly get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000A1 RID: 161 RVA: 0x00004530 File Offset: 0x00002730
		// (set) Token: 0x060000A2 RID: 162 RVA: 0x00004538 File Offset: 0x00002738
		public DateTime Timestamp { readonly get; set; }
	}
}
