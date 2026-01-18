using System;

namespace Networking.UDP
{
	// Token: 0x02000031 RID: 49
	public readonly ref struct PooledPacket
	{
		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000163 RID: 355 RVA: 0x000088EA File Offset: 0x00006AEA
		public byte[] Data
		{
			get
			{
				return this._packet.RawData;
			}
		}

		// Token: 0x06000164 RID: 356 RVA: 0x000088F7 File Offset: 0x00006AF7
		internal PooledPacket(NetPacket packet, int maxDataSize, byte channelNumber)
		{
			this._packet = packet;
			this.UserDataOffset = this._packet.GetHeaderSize();
			this._packet.Size = this.UserDataOffset;
			this.MaxUserDataSize = maxDataSize - this.UserDataOffset;
			this._channelNumber = channelNumber;
		}

		// Token: 0x0400013C RID: 316
		internal readonly NetPacket _packet;

		// Token: 0x0400013D RID: 317
		internal readonly byte _channelNumber;

		// Token: 0x0400013E RID: 318
		public readonly int MaxUserDataSize;

		// Token: 0x0400013F RID: 319
		public readonly int UserDataOffset;
	}
}
