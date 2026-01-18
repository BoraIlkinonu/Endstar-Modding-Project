using System;
using System.Collections.Generic;

namespace Endless.Networking.TCP
{
	// Token: 0x02000018 RID: 24
	public class ServerTcpPacketHandler : IServerTcpPacketHandler, ITcpPacketHandler
	{
		// Token: 0x060000C4 RID: 196 RVA: 0x00004BD4 File Offset: 0x00002DD4
		public void Initialize<T>(Server<T> server, Dictionary<int, Func<DataBuffer, Message>> readCollection, Action<int, Message> receiveCallback) where T : IServerTcpPacketHandler, new()
		{
			this.Server = server as Server<ServerTcpPacketHandler>;
			bool flag = this.Server == null;
			if (flag)
			{
				throw new InvalidCastException("Invalid type for packet handler.");
			}
			this.ReceiveCallback = receiveCallback;
			this.ReadCollection = readCollection;
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00004C18 File Offset: 0x00002E18
		public void HandlePacket(DataBuffer packetBuffer)
		{
			int num = -1;
			Message message = null;
			try
			{
				num = packetBuffer.ReadInteger(true);
				int num2 = packetBuffer.ReadInteger(true);
				bool flag = num2 == 0;
				if (flag)
				{
					return;
				}
				message = this.ReadCollection[num2](packetBuffer);
			}
			catch (Exception ex)
			{
				Logger.Log(this.Server.LogFileName, ex.ToString(), true);
				packetBuffer.Dispose();
				return;
			}
			packetBuffer.Dispose();
			Action<int, Message> receiveCallback = this.ReceiveCallback;
			if (receiveCallback != null)
			{
				receiveCallback(num, message);
			}
		}

		// Token: 0x0400005D RID: 93
		private Server<ServerTcpPacketHandler> Server;

		// Token: 0x0400005E RID: 94
		private Action<int, Message> ReceiveCallback;

		// Token: 0x0400005F RID: 95
		private Dictionary<int, Func<DataBuffer, Message>> ReadCollection;
	}
}
