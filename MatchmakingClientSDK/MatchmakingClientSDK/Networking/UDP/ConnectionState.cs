using System;

namespace Networking.UDP
{
	// Token: 0x02000028 RID: 40
	[Flags]
	public enum ConnectionState : byte
	{
		// Token: 0x040000EA RID: 234
		Outgoing = 2,
		// Token: 0x040000EB RID: 235
		Connected = 4,
		// Token: 0x040000EC RID: 236
		ShutdownRequested = 8,
		// Token: 0x040000ED RID: 237
		Disconnected = 16,
		// Token: 0x040000EE RID: 238
		EndPointChange = 32,
		// Token: 0x040000EF RID: 239
		Any = 46
	}
}
