using System;

namespace Networking.UDP.Utils
{
	// Token: 0x02000041 RID: 65
	public class NtpPacket
	{
		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000258 RID: 600 RVA: 0x0000B6DF File Offset: 0x000098DF
		public byte[] Bytes { get; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000259 RID: 601 RVA: 0x0000B6E7 File Offset: 0x000098E7
		public NtpLeapIndicator LeapIndicator
		{
			get
			{
				return (NtpLeapIndicator)((this.Bytes[0] & 192) >> 6);
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600025A RID: 602 RVA: 0x0000B6F9 File Offset: 0x000098F9
		// (set) Token: 0x0600025B RID: 603 RVA: 0x0000B708 File Offset: 0x00009908
		public int VersionNumber
		{
			get
			{
				return (this.Bytes[0] & 56) >> 3;
			}
			private set
			{
				this.Bytes[0] = (byte)(((int)this.Bytes[0] & -57) | (value << 3));
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x0600025C RID: 604 RVA: 0x0000B722 File Offset: 0x00009922
		// (set) Token: 0x0600025D RID: 605 RVA: 0x0000B72E File Offset: 0x0000992E
		public NtpMode Mode
		{
			get
			{
				return (NtpMode)(this.Bytes[0] & 7);
			}
			private set
			{
				this.Bytes[0] = (byte)(((NtpMode)this.Bytes[0] & (NtpMode)(-8)) | value);
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x0600025E RID: 606 RVA: 0x0000B746 File Offset: 0x00009946
		public int Stratum
		{
			get
			{
				return (int)this.Bytes[1];
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x0600025F RID: 607 RVA: 0x0000B750 File Offset: 0x00009950
		public int Poll
		{
			get
			{
				return (int)this.Bytes[2];
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000260 RID: 608 RVA: 0x0000B75A File Offset: 0x0000995A
		public int Precision
		{
			get
			{
				return (int)((sbyte)this.Bytes[3]);
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x06000261 RID: 609 RVA: 0x0000B765 File Offset: 0x00009965
		public TimeSpan RootDelay
		{
			get
			{
				return this.GetTimeSpan32(4);
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x06000262 RID: 610 RVA: 0x0000B76E File Offset: 0x0000996E
		public TimeSpan RootDispersion
		{
			get
			{
				return this.GetTimeSpan32(8);
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x06000263 RID: 611 RVA: 0x0000B777 File Offset: 0x00009977
		public uint ReferenceId
		{
			get
			{
				return this.GetUInt32BE(12);
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x06000264 RID: 612 RVA: 0x0000B781 File Offset: 0x00009981
		public DateTime? ReferenceTimestamp
		{
			get
			{
				return this.GetDateTime64(16);
			}
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000265 RID: 613 RVA: 0x0000B78B File Offset: 0x0000998B
		public DateTime? OriginTimestamp
		{
			get
			{
				return this.GetDateTime64(24);
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000266 RID: 614 RVA: 0x0000B795 File Offset: 0x00009995
		public DateTime? ReceiveTimestamp
		{
			get
			{
				return this.GetDateTime64(32);
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000267 RID: 615 RVA: 0x0000B79F File Offset: 0x0000999F
		// (set) Token: 0x06000268 RID: 616 RVA: 0x0000B7A9 File Offset: 0x000099A9
		public DateTime? TransmitTimestamp
		{
			get
			{
				return this.GetDateTime64(40);
			}
			private set
			{
				this.SetDateTime64(40, value);
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000269 RID: 617 RVA: 0x0000B7B4 File Offset: 0x000099B4
		// (set) Token: 0x0600026A RID: 618 RVA: 0x0000B7BC File Offset: 0x000099BC
		public DateTime? DestinationTimestamp { get; private set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x0600026B RID: 619 RVA: 0x0000B7C8 File Offset: 0x000099C8
		public TimeSpan RoundTripTime
		{
			get
			{
				this.CheckTimestamps();
				return this.ReceiveTimestamp.Value - this.OriginTimestamp.Value + (this.DestinationTimestamp.Value - this.TransmitTimestamp.Value);
			}
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x0600026C RID: 620 RVA: 0x0000B824 File Offset: 0x00009A24
		public TimeSpan CorrectionOffset
		{
			get
			{
				this.CheckTimestamps();
				return TimeSpan.FromTicks((this.ReceiveTimestamp.Value - this.OriginTimestamp.Value - (this.DestinationTimestamp.Value - this.TransmitTimestamp.Value)).Ticks / 2L);
			}
		}

		// Token: 0x0600026D RID: 621 RVA: 0x0000B88E File Offset: 0x00009A8E
		public NtpPacket()
			: this(new byte[48])
		{
			this.Mode = NtpMode.Client;
			this.VersionNumber = 4;
			this.TransmitTimestamp = new DateTime?(DateTime.UtcNow);
		}

		// Token: 0x0600026E RID: 622 RVA: 0x0000B8BB File Offset: 0x00009ABB
		internal NtpPacket(byte[] bytes)
		{
			if (bytes.Length < 48)
			{
				throw new ArgumentException("SNTP reply packet must be at least 48 bytes long.", "bytes");
			}
			this.Bytes = bytes;
		}

		// Token: 0x0600026F RID: 623 RVA: 0x0000B8E1 File Offset: 0x00009AE1
		public static NtpPacket FromServerResponse(byte[] bytes, DateTime destinationTimestamp)
		{
			return new NtpPacket(bytes)
			{
				DestinationTimestamp = new DateTime?(destinationTimestamp)
			};
		}

		// Token: 0x06000270 RID: 624 RVA: 0x0000B8F8 File Offset: 0x00009AF8
		internal void ValidateRequest()
		{
			if (this.Mode != NtpMode.Client)
			{
				throw new InvalidOperationException("This is not a request SNTP packet.");
			}
			if (this.VersionNumber == 0)
			{
				throw new InvalidOperationException("Protocol version of the request is not specified.");
			}
			if (this.TransmitTimestamp == null)
			{
				throw new InvalidOperationException("TransmitTimestamp must be set in request packet.");
			}
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0000B948 File Offset: 0x00009B48
		internal void ValidateReply()
		{
			if (this.Mode != NtpMode.Server)
			{
				throw new InvalidOperationException("This is not a reply SNTP packet.");
			}
			if (this.VersionNumber == 0)
			{
				throw new InvalidOperationException("Protocol version of the reply is not specified.");
			}
			if (this.Stratum == 0)
			{
				throw new InvalidOperationException(string.Format("Received Kiss-o'-Death SNTP packet with code 0x{0:x}.", this.ReferenceId));
			}
			if (this.LeapIndicator == NtpLeapIndicator.AlarmCondition)
			{
				throw new InvalidOperationException("SNTP server has unsynchronized clock.");
			}
			this.CheckTimestamps();
		}

		// Token: 0x06000272 RID: 626 RVA: 0x0000B9BC File Offset: 0x00009BBC
		private void CheckTimestamps()
		{
			if (this.OriginTimestamp == null)
			{
				throw new InvalidOperationException("Origin timestamp is missing.");
			}
			if (this.ReceiveTimestamp == null)
			{
				throw new InvalidOperationException("Receive timestamp is missing.");
			}
			if (this.TransmitTimestamp == null)
			{
				throw new InvalidOperationException("Transmit timestamp is missing.");
			}
			if (this.DestinationTimestamp == null)
			{
				throw new InvalidOperationException("Destination timestamp is missing.");
			}
		}

		// Token: 0x06000273 RID: 627 RVA: 0x0000BA38 File Offset: 0x00009C38
		private DateTime? GetDateTime64(int offset)
		{
			ulong uint64BE = this.GetUInt64BE(offset);
			if (uint64BE == 0UL)
			{
				return null;
			}
			return new DateTime?(new DateTime(NtpPacket.Epoch.Ticks + Convert.ToInt64(uint64BE * 0.0023283064365386963)));
		}

		// Token: 0x06000274 RID: 628 RVA: 0x0000BA84 File Offset: 0x00009C84
		private void SetDateTime64(int offset, DateTime? value)
		{
			this.SetUInt64BE(offset, (value == null) ? 0UL : Convert.ToUInt64((double)(value.Value.Ticks - NtpPacket.Epoch.Ticks) * 429.4967296));
		}

		// Token: 0x06000275 RID: 629 RVA: 0x0000BAD2 File Offset: 0x00009CD2
		private TimeSpan GetTimeSpan32(int offset)
		{
			return TimeSpan.FromSeconds((double)this.GetInt32BE(offset) / 65536.0);
		}

		// Token: 0x06000276 RID: 630 RVA: 0x0000BAEB File Offset: 0x00009CEB
		private ulong GetUInt64BE(int offset)
		{
			return NtpPacket.SwapEndianness(BitConverter.ToUInt64(this.Bytes, offset));
		}

		// Token: 0x06000277 RID: 631 RVA: 0x0000BAFE File Offset: 0x00009CFE
		private void SetUInt64BE(int offset, ulong value)
		{
			FastBitConverter.GetBytes(this.Bytes, offset, NtpPacket.SwapEndianness(value));
		}

		// Token: 0x06000278 RID: 632 RVA: 0x0000BB12 File Offset: 0x00009D12
		private int GetInt32BE(int offset)
		{
			return (int)this.GetUInt32BE(offset);
		}

		// Token: 0x06000279 RID: 633 RVA: 0x0000BB1B File Offset: 0x00009D1B
		private uint GetUInt32BE(int offset)
		{
			return NtpPacket.SwapEndianness(BitConverter.ToUInt32(this.Bytes, offset));
		}

		// Token: 0x0600027A RID: 634 RVA: 0x0000BB2E File Offset: 0x00009D2E
		private static uint SwapEndianness(uint x)
		{
			return ((x & 255U) << 24) | ((x & 65280U) << 8) | ((x & 16711680U) >> 8) | ((x & 4278190080U) >> 24);
		}

		// Token: 0x0600027B RID: 635 RVA: 0x0000BB59 File Offset: 0x00009D59
		private static ulong SwapEndianness(ulong x)
		{
			return ((ulong)NtpPacket.SwapEndianness((uint)x) << 32) | (ulong)NtpPacket.SwapEndianness((uint)(x >> 32));
		}

		// Token: 0x0400017A RID: 378
		private static readonly DateTime Epoch = new DateTime(1900, 1, 1);
	}
}
