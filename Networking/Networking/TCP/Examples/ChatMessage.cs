using System;

namespace Endless.Networking.TCP.Examples
{
	// Token: 0x0200001D RID: 29
	public class ChatMessage : Message
	{
		// Token: 0x0600010B RID: 267 RVA: 0x00005800 File Offset: 0x00003A00
		public static ChatMessage Read(DataBuffer packetBuffer)
		{
			return new ChatMessage
			{
				Content = packetBuffer.ReadString(true)
			};
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00005828 File Offset: 0x00003A28
		public static DataBuffer Write(string content)
		{
			DataBuffer dataBuffer = new DataBuffer(1000);
			dataBuffer.WriteString(content);
			return dataBuffer;
		}

		// Token: 0x04000077 RID: 119
		public string Content;
	}
}
