using System;

namespace Networking.UDP
{
	// Token: 0x02000026 RID: 38
	internal enum PacketProperty : byte
	{
		// Token: 0x040000D1 RID: 209
		Unreliable,
		// Token: 0x040000D2 RID: 210
		Channeled,
		// Token: 0x040000D3 RID: 211
		Ack,
		// Token: 0x040000D4 RID: 212
		Ping,
		// Token: 0x040000D5 RID: 213
		Pong,
		// Token: 0x040000D6 RID: 214
		ConnectRequest,
		// Token: 0x040000D7 RID: 215
		ConnectAccept,
		// Token: 0x040000D8 RID: 216
		Disconnect,
		// Token: 0x040000D9 RID: 217
		UnconnectedMessage,
		// Token: 0x040000DA RID: 218
		MtuCheck,
		// Token: 0x040000DB RID: 219
		MtuOk,
		// Token: 0x040000DC RID: 220
		Broadcast,
		// Token: 0x040000DD RID: 221
		Merged,
		// Token: 0x040000DE RID: 222
		ShutdownOk,
		// Token: 0x040000DF RID: 223
		PeerNotFound,
		// Token: 0x040000E0 RID: 224
		InvalidProtocol,
		// Token: 0x040000E1 RID: 225
		NatMessage,
		// Token: 0x040000E2 RID: 226
		Empty
	}
}
