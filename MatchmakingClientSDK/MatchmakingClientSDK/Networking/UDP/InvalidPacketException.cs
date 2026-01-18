using System;

namespace Networking.UDP
{
	// Token: 0x0200001E RID: 30
	public class InvalidPacketException : ArgumentException
	{
		// Token: 0x06000078 RID: 120 RVA: 0x0000347F File Offset: 0x0000167F
		public InvalidPacketException(string message)
			: base(message)
		{
		}
	}
}
