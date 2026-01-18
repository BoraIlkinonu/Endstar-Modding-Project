using System;

namespace Endless.Networking.TCP.Examples
{
	// Token: 0x0200001B RID: 27
	public static class ExampleConfig
	{
		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000FE RID: 254 RVA: 0x00005775 File Offset: 0x00003975
		// (set) Token: 0x060000FF RID: 255 RVA: 0x0000577C File Offset: 0x0000397C
		public static string LogFileName { get; set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000100 RID: 256 RVA: 0x00005784 File Offset: 0x00003984
		// (set) Token: 0x06000101 RID: 257 RVA: 0x0000578B File Offset: 0x0000398B
		public static string ServerIp { get; set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000102 RID: 258 RVA: 0x00005793 File Offset: 0x00003993
		// (set) Token: 0x06000103 RID: 259 RVA: 0x0000579A File Offset: 0x0000399A
		public static int ServerPort { get; set; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000104 RID: 260 RVA: 0x000057A2 File Offset: 0x000039A2
		// (set) Token: 0x06000105 RID: 261 RVA: 0x000057A9 File Offset: 0x000039A9
		public static int ConnectionTimeout { get; set; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000106 RID: 262 RVA: 0x000057B1 File Offset: 0x000039B1
		// (set) Token: 0x06000107 RID: 263 RVA: 0x000057B8 File Offset: 0x000039B8
		public static int MaxConnections { get; set; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000108 RID: 264 RVA: 0x000057C0 File Offset: 0x000039C0
		// (set) Token: 0x06000109 RID: 265 RVA: 0x000057C7 File Offset: 0x000039C7
		public static int QueueSize { get; set; }
	}
}
