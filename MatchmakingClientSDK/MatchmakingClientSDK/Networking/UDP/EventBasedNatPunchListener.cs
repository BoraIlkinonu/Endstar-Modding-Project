using System;
using System.Net;

namespace Networking.UDP
{
	// Token: 0x0200001A RID: 26
	public class EventBasedNatPunchListener : INatPunchListener
	{
		// Token: 0x1400000B RID: 11
		// (add) Token: 0x06000065 RID: 101 RVA: 0x00002F14 File Offset: 0x00001114
		// (remove) Token: 0x06000066 RID: 102 RVA: 0x00002F4C File Offset: 0x0000114C
		public event EventBasedNatPunchListener.OnNatIntroductionRequest NatIntroductionRequest;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x06000067 RID: 103 RVA: 0x00002F84 File Offset: 0x00001184
		// (remove) Token: 0x06000068 RID: 104 RVA: 0x00002FBC File Offset: 0x000011BC
		public event EventBasedNatPunchListener.OnNatIntroductionSuccess NatIntroductionSuccess;

		// Token: 0x06000069 RID: 105 RVA: 0x00002FF1 File Offset: 0x000011F1
		void INatPunchListener.OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
		{
			if (this.NatIntroductionRequest != null)
			{
				this.NatIntroductionRequest(localEndPoint, remoteEndPoint, token);
			}
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00003009 File Offset: 0x00001209
		void INatPunchListener.OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
		{
			if (this.NatIntroductionSuccess != null)
			{
				this.NatIntroductionSuccess(targetEndPoint, type, token);
			}
		}

		// Token: 0x0200007E RID: 126
		// (Invoke) Token: 0x06000460 RID: 1120
		public delegate void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token);

		// Token: 0x0200007F RID: 127
		// (Invoke) Token: 0x06000464 RID: 1124
		public delegate void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token);
	}
}
