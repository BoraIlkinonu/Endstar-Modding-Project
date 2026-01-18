using System;
using System.Collections.Generic;

namespace Endless.Networking.TCP.Examples
{
	// Token: 0x0200001C RID: 28
	public static class ExampleMessages
	{
		// Token: 0x04000076 RID: 118
		public static Dictionary<int, Func<DataBuffer, Message>> MessageReaders = new Dictionary<int, Func<DataBuffer, Message>> { 
		{
			1,
			new Func<DataBuffer, Message>(ChatMessage.Read)
		} };

		// Token: 0x0200003B RID: 59
		public enum Messages
		{
			// Token: 0x040000CD RID: 205
			KeepAlive,
			// Token: 0x040000CE RID: 206
			Chat
		}
	}
}
