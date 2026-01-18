using System;

namespace Networking.UDP
{
	// Token: 0x02000033 RID: 51
	public enum RelayMessages : byte
	{
		// Token: 0x04000142 RID: 322
		ClientConnected,
		// Token: 0x04000143 RID: 323
		ClientDisconnected,
		// Token: 0x04000144 RID: 324
		Data,
		// Token: 0x04000145 RID: 325
		DisconnectClient
	}
}
