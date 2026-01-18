using System;
using System.Net;
using Networking.UDP.Utils;

namespace Networking.UDP.Layers
{
	// Token: 0x02000046 RID: 70
	public sealed class Crc32cLayer : PacketLayerBase
	{
		// Token: 0x06000281 RID: 641 RVA: 0x0000BC3C File Offset: 0x00009E3C
		public Crc32cLayer()
			: base(4)
		{
		}

		// Token: 0x06000282 RID: 642 RVA: 0x0000BC48 File Offset: 0x00009E48
		public override void ProcessInboundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int length)
		{
			if (length < 5)
			{
				NetDebug.WriteError("[NM] DataReceived size: bad!");
				length = 0;
				return;
			}
			int num = length - 4;
			if (CRC32C.Compute(data, 0, num) != BitConverter.ToUInt32(data, num))
			{
				length = 0;
				return;
			}
			length -= 4;
		}

		// Token: 0x06000283 RID: 643 RVA: 0x0000BC8A File Offset: 0x00009E8A
		public override void ProcessOutBoundPacket(ref IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
		{
			FastBitConverter.GetBytes(data, length, CRC32C.Compute(data, offset, length));
			length += 4;
		}
	}
}
