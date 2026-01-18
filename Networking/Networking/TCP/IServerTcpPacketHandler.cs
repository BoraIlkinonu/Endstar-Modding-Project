using System;
using System.Collections.Generic;

namespace Endless.Networking.TCP
{
	// Token: 0x02000014 RID: 20
	public interface IServerTcpPacketHandler : ITcpPacketHandler
	{
		// Token: 0x06000095 RID: 149
		void Initialize<T>(Server<T> server, Dictionary<int, Func<DataBuffer, Message>> readCollection, Action<int, Message> receiveCallback) where T : IServerTcpPacketHandler, new();
	}
}
