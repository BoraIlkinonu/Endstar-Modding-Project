using System;
using System.Net;

namespace Networking.UDP
{
	// Token: 0x02000013 RID: 19
	public interface IPeerAddressChangedListener
	{
		// Token: 0x0600002B RID: 43
		void OnPeerAddressChanged(NetPeer peer, IPEndPoint previousAddress);
	}
}
