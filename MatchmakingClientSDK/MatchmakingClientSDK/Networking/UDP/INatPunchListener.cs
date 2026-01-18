using System;
using System.Net;

namespace Networking.UDP
{
	// Token: 0x02000019 RID: 25
	public interface INatPunchListener
	{
		// Token: 0x06000063 RID: 99
		void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token);

		// Token: 0x06000064 RID: 100
		void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token);
	}
}
