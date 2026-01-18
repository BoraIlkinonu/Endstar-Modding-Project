using System;

namespace Networking.UDP
{
	// Token: 0x0200001D RID: 29
	public static class NetConstants
	{
		// Token: 0x0400005A RID: 90
		public const int DefaultWindowSize = 64;

		// Token: 0x0400005B RID: 91
		public const int SocketBufferSize = 1048576;

		// Token: 0x0400005C RID: 92
		public const int SocketTTL = 255;

		// Token: 0x0400005D RID: 93
		public const int HeaderSize = 1;

		// Token: 0x0400005E RID: 94
		public const int ChanneledHeaderSize = 4;

		// Token: 0x0400005F RID: 95
		public const int FragmentHeaderSize = 6;

		// Token: 0x04000060 RID: 96
		public const int FragmentedHeaderTotalSize = 10;

		// Token: 0x04000061 RID: 97
		public const ushort MaxSequence = 32768;

		// Token: 0x04000062 RID: 98
		public const ushort HalfMaxSequence = 16384;

		// Token: 0x04000063 RID: 99
		internal const int ProtocolId = 13;

		// Token: 0x04000064 RID: 100
		internal const int MaxUdpHeaderSize = 68;

		// Token: 0x04000065 RID: 101
		internal const int ChannelTypeCount = 4;

		// Token: 0x04000066 RID: 102
		internal static readonly int[] PossibleMtu = new int[] { 1404, 1424, 1432 };

		// Token: 0x04000067 RID: 103
		public static readonly int InitialMtu = NetConstants.PossibleMtu[0];

		// Token: 0x04000068 RID: 104
		public static readonly int MaxPacketSize = NetConstants.PossibleMtu[NetConstants.PossibleMtu.Length - 1];

		// Token: 0x04000069 RID: 105
		public static readonly int MaxUnreliableDataSize = NetConstants.MaxPacketSize - 1;

		// Token: 0x0400006A RID: 106
		public const byte MaxConnectionNumber = 64;
	}
}
