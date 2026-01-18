using System;

namespace MatchmakingClientSDK
{
	// Token: 0x0200005B RID: 91
	public readonly struct ConnectionData
	{
		// Token: 0x06000373 RID: 883 RVA: 0x0000F584 File Offset: 0x0000D784
		public ConnectionData(string ip, int port, string key)
		{
			this.Ip = ip;
			this.Port = port;
			this.Key = key;
		}

		// Token: 0x04000244 RID: 580
		public readonly string Ip;

		// Token: 0x04000245 RID: 581
		public readonly int Port;

		// Token: 0x04000246 RID: 582
		public readonly string Key;
	}
}
