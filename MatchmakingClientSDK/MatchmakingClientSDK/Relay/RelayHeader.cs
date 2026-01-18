using System;
using Networking.UDP.Utils;

namespace Relay
{
	// Token: 0x02000009 RID: 9
	public struct RelayHeader
	{
		// Token: 0x06000008 RID: 8 RVA: 0x000020A6 File Offset: 0x000002A6
		public void Serialize(NetDataWriter writer)
		{
			writer.Put((byte)this.MessageType);
			writer.Put(this.Channel);
			writer.Put(this.Key);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000020CC File Offset: 0x000002CC
		public static RelayHeader Deserialize(byte[] data)
		{
			int num = 0;
			return new RelayHeader
			{
				MessageType = (RelayMessageType)data[num++],
				Channel = data[num++],
				Key = BitConverter.ToUInt64(data, num)
			};
		}

		// Token: 0x04000006 RID: 6
		public const int SIZE = 10;

		// Token: 0x04000007 RID: 7
		public RelayMessageType MessageType;

		// Token: 0x04000008 RID: 8
		public byte Channel;

		// Token: 0x04000009 RID: 9
		public ulong Key;
	}
}
