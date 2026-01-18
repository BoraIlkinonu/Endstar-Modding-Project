using System;
using System.Net;

namespace Networking.UDP.Layers
{
	// Token: 0x02000047 RID: 71
	public abstract class PacketLayerBase
	{
		// Token: 0x06000284 RID: 644 RVA: 0x0000BCAA File Offset: 0x00009EAA
		protected PacketLayerBase(int extraPacketSizeForLayer)
		{
			this.ExtraPacketSizeForLayer = extraPacketSizeForLayer;
		}

		// Token: 0x06000285 RID: 645
		public abstract void ProcessInboundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int length);

		// Token: 0x06000286 RID: 646
		public abstract void ProcessOutBoundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length);

		// Token: 0x0400018B RID: 395
		public readonly int ExtraPacketSizeForLayer;
	}
}
