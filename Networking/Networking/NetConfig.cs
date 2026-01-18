using System;

namespace Endless.Networking
{
	// Token: 0x0200000D RID: 13
	public static class NetConfig
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000044 RID: 68 RVA: 0x00003056 File Offset: 0x00001256
		// (set) Token: 0x06000045 RID: 69 RVA: 0x0000305D File Offset: 0x0000125D
		public static string LogFileName { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000046 RID: 70 RVA: 0x00003065 File Offset: 0x00001265
		// (set) Token: 0x06000047 RID: 71 RVA: 0x0000306C File Offset: 0x0000126C
		public static InstanceType InstanceType { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000048 RID: 72 RVA: 0x00003074 File Offset: 0x00001274
		// (set) Token: 0x06000049 RID: 73 RVA: 0x0000307B File Offset: 0x0000127B
		public static string InstanceName { get; set; } = "MP Instance";

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00003083 File Offset: 0x00001283
		// (set) Token: 0x0600004B RID: 75 RVA: 0x0000308A File Offset: 0x0000128A
		public static string InstanceIp { get; set; } = "";

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600004C RID: 76 RVA: 0x00003092 File Offset: 0x00001292
		// (set) Token: 0x0600004D RID: 77 RVA: 0x00003099 File Offset: 0x00001299
		public static string LocalInstanceIp { get; set; } = "";

		// Token: 0x04000020 RID: 32
		public const string CONFIG_FILE = "NetConfig";
	}
}
