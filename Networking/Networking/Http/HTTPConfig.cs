using System;

namespace Endless.Networking.Http
{
	// Token: 0x02000020 RID: 32
	public static class HTTPConfig
	{
		// Token: 0x17000032 RID: 50
		// (get) Token: 0x0600011C RID: 284 RVA: 0x00005997 File Offset: 0x00003B97
		// (set) Token: 0x0600011D RID: 285 RVA: 0x0000599E File Offset: 0x00003B9E
		public static string ServerPrefix
		{
			get
			{
				return HTTPConfig.serverPrefix;
			}
			set
			{
				HTTPConfig.serverPrefix = value;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600011E RID: 286 RVA: 0x000059A6 File Offset: 0x00003BA6
		// (set) Token: 0x0600011F RID: 287 RVA: 0x000059AD File Offset: 0x00003BAD
		public static int ServerPort
		{
			get
			{
				return HTTPConfig.serverPort;
			}
			set
			{
				HTTPConfig.serverPort = value;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000120 RID: 288 RVA: 0x000059B5 File Offset: 0x00003BB5
		// (set) Token: 0x06000121 RID: 289 RVA: 0x000059BC File Offset: 0x00003BBC
		public static string LogFileName { get; set; }

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000122 RID: 290 RVA: 0x000059C4 File Offset: 0x00003BC4
		// (set) Token: 0x06000123 RID: 291 RVA: 0x000059CB File Offset: 0x00003BCB
		public static bool DirectoryPermission { get; set; }

		// Token: 0x04000079 RID: 121
		public const string CONFIG_FILE = "HttpConfig";

		// Token: 0x0400007A RID: 122
		public const string NAME = "Endless_HTTPServer";

		// Token: 0x0400007B RID: 123
		public const string VERSION = "HTTP/1.1";

		// Token: 0x0400007C RID: 124
		private static string serverPrefix = "http://localhost";

		// Token: 0x0400007D RID: 125
		private static int serverPort = 80;

		// Token: 0x0400007E RID: 126
		public static string StatusTemplatePath = "templates/StatusTemplate.html";

		// Token: 0x0400007F RID: 127
		public static string OKTemplatePath = "templates/OKTemplate.html";
	}
}
