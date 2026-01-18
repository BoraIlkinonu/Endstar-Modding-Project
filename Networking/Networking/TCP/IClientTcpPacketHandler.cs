using System;
using System.Collections.Generic;

namespace Endless.Networking.TCP
{
	// Token: 0x02000015 RID: 21
	public interface IClientTcpPacketHandler : ITcpPacketHandler
	{
		// Token: 0x06000096 RID: 150
		void Initialize<T>(Client<T> client, Dictionary<int, Func<DataBuffer, Message>> readCollection, Action<Message> receiveCallback) where T : IClientTcpPacketHandler, new();
	}
}
