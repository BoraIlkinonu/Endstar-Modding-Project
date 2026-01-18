using System;

namespace Networking.UDP
{
	// Token: 0x02000011 RID: 17
	public interface IDeliveryEventListener
	{
		// Token: 0x06000029 RID: 41
		void OnMessageDelivered(NetPeer peer, object userData);
	}
}
