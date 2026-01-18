using System;

namespace Networking.UDP
{
	// Token: 0x0200001F RID: 31
	public class TooBigPacketException : InvalidPacketException
	{
		// Token: 0x06000079 RID: 121 RVA: 0x00003488 File Offset: 0x00001688
		public TooBigPacketException(string message)
			: base(message)
		{
		}
	}
}
