using System;

namespace Endless.Networking.TCP
{
	// Token: 0x02000013 RID: 19
	public interface ITcpPacketHandler
	{
		// Token: 0x06000094 RID: 148
		void HandlePacket(DataBuffer packetBuffer);
	}
}
