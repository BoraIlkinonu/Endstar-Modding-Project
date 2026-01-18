using System;
using System.Net;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x02000015 RID: 21
	internal sealed class NetConnectRequestPacket
	{
		// Token: 0x06000055 RID: 85 RVA: 0x00002946 File Offset: 0x00000B46
		private NetConnectRequestPacket(long connectionTime, byte connectionNumber, int localId, byte[] targetAddress, NetDataReader data)
		{
			this.ConnectionTime = connectionTime;
			this.ConnectionNumber = connectionNumber;
			this.TargetAddress = targetAddress;
			this.Data = data;
			this.PeerId = localId;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00002973 File Offset: 0x00000B73
		public static int GetProtocolId(NetPacket packet)
		{
			return BitConverter.ToInt32(packet.RawData, 1);
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00002984 File Offset: 0x00000B84
		public static NetConnectRequestPacket FromData(NetPacket packet)
		{
			if (packet.ConnectionNumber >= 64)
			{
				return null;
			}
			long num = BitConverter.ToInt64(packet.RawData, 5);
			int num2 = BitConverter.ToInt32(packet.RawData, 13);
			int num3 = (int)packet.RawData[17];
			if (num3 != 16 && num3 != 28)
			{
				return null;
			}
			byte[] array = new byte[num3];
			Buffer.BlockCopy(packet.RawData, 18, array, 0, num3);
			NetDataReader netDataReader = new NetDataReader(null, 0, 0);
			if (packet.Size > 18 + num3)
			{
				netDataReader.SetSource(packet.RawData, 18 + num3, packet.Size);
			}
			return new NetConnectRequestPacket(num, packet.ConnectionNumber, num2, array, netDataReader);
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00002A24 File Offset: 0x00000C24
		public static NetPacket Make(NetDataWriter connectData, SocketAddress addressBytes, long connectTime, int localId)
		{
			NetPacket netPacket = new NetPacket(PacketProperty.ConnectRequest, connectData.Length + addressBytes.Size);
			FastBitConverter.GetBytes(netPacket.RawData, 1, 13);
			FastBitConverter.GetBytes(netPacket.RawData, 5, connectTime);
			FastBitConverter.GetBytes(netPacket.RawData, 13, localId);
			netPacket.RawData[17] = (byte)addressBytes.Size;
			for (int i = 0; i < addressBytes.Size; i++)
			{
				netPacket.RawData[18 + i] = addressBytes[i];
			}
			Buffer.BlockCopy(connectData.Data, 0, netPacket.RawData, 18 + addressBytes.Size, connectData.Length);
			return netPacket;
		}

		// Token: 0x04000034 RID: 52
		public const int HeaderSize = 18;

		// Token: 0x04000035 RID: 53
		public readonly long ConnectionTime;

		// Token: 0x04000036 RID: 54
		public byte ConnectionNumber;

		// Token: 0x04000037 RID: 55
		public readonly byte[] TargetAddress;

		// Token: 0x04000038 RID: 56
		public readonly NetDataReader Data;

		// Token: 0x04000039 RID: 57
		public readonly int PeerId;
	}
}
