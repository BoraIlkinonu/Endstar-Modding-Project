using System;

namespace Networking.UDP
{
	// Token: 0x0200000E RID: 14
	public enum DisconnectReason
	{
		// Token: 0x0400001B RID: 27
		ConnectionFailed,
		// Token: 0x0400001C RID: 28
		Timeout,
		// Token: 0x0400001D RID: 29
		HostUnreachable,
		// Token: 0x0400001E RID: 30
		NetworkUnreachable,
		// Token: 0x0400001F RID: 31
		RemoteConnectionClose,
		// Token: 0x04000020 RID: 32
		DisconnectPeerCalled,
		// Token: 0x04000021 RID: 33
		ConnectionRejected,
		// Token: 0x04000022 RID: 34
		InvalidProtocol,
		// Token: 0x04000023 RID: 35
		UnknownHost,
		// Token: 0x04000024 RID: 36
		Reconnect,
		// Token: 0x04000025 RID: 37
		PeerToPeerConnection,
		// Token: 0x04000026 RID: 38
		PeerNotFound
	}
}
