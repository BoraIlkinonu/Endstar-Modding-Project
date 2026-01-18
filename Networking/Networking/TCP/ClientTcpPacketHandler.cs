using System;
using System.Collections.Generic;

namespace Endless.Networking.TCP
{
	// Token: 0x02000012 RID: 18
	public class ClientTcpPacketHandler : IClientTcpPacketHandler, ITcpPacketHandler
	{
		// Token: 0x06000091 RID: 145 RVA: 0x00003F10 File Offset: 0x00002110
		public void Initialize<T>(Client<T> client, Dictionary<int, Func<DataBuffer, Message>> readCollection, Action<Message> receiveCallback) where T : IClientTcpPacketHandler, new()
		{
			this.Client = client as Client<ClientTcpPacketHandler>;
			bool flag = this.Client == null;
			if (flag)
			{
				throw new InvalidCastException("Invalid type for packet handler.");
			}
			this.ReceiveCallback = receiveCallback;
			this.ReadCollection = readCollection;
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00003F54 File Offset: 0x00002154
		public void HandlePacket(DataBuffer packetBuffer)
		{
			Message message = null;
			try
			{
				int num = packetBuffer.ReadInteger(true);
				bool flag = num == 0;
				if (flag)
				{
					return;
				}
				message = this.ReadCollection[num](packetBuffer);
			}
			catch (Exception ex)
			{
				Logger.Log(this.Client.LogFileName, ex.ToString(), true);
				return;
			}
			Action<Message> receiveCallback = this.ReceiveCallback;
			if (receiveCallback != null)
			{
				receiveCallback(message);
			}
		}

		// Token: 0x04000044 RID: 68
		private Client<ClientTcpPacketHandler> Client;

		// Token: 0x04000045 RID: 69
		private Action<Message> ReceiveCallback;

		// Token: 0x04000046 RID: 70
		private Dictionary<int, Func<DataBuffer, Message>> ReadCollection;
	}
}
