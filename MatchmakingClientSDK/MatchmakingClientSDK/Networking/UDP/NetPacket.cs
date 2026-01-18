using System;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x02000027 RID: 39
	internal sealed class NetPacket
	{
		// Token: 0x060000F8 RID: 248 RVA: 0x000067D0 File Offset: 0x000049D0
		static NetPacket()
		{
			for (int i = 0; i < NetPacket.HeaderSizes.Length; i++)
			{
				switch ((byte)i)
				{
				case 1:
				case 2:
					NetPacket.HeaderSizes[i] = 4;
					break;
				case 3:
					NetPacket.HeaderSizes[i] = 3;
					break;
				case 4:
					NetPacket.HeaderSizes[i] = 11;
					break;
				case 5:
					NetPacket.HeaderSizes[i] = 18;
					break;
				case 6:
					NetPacket.HeaderSizes[i] = 15;
					break;
				case 7:
					NetPacket.HeaderSizes[i] = 9;
					break;
				default:
					NetPacket.HeaderSizes[i] = 1;
					break;
				}
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x060000F9 RID: 249 RVA: 0x00006888 File Offset: 0x00004A88
		// (set) Token: 0x060000FA RID: 250 RVA: 0x00006896 File Offset: 0x00004A96
		public PacketProperty Property
		{
			get
			{
				return (PacketProperty)(this.RawData[0] & 31);
			}
			set
			{
				this.RawData[0] = (this.RawData[0] & 224) | (byte)value;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x060000FB RID: 251 RVA: 0x000068B1 File Offset: 0x00004AB1
		// (set) Token: 0x060000FC RID: 252 RVA: 0x000068C1 File Offset: 0x00004AC1
		public byte ConnectionNumber
		{
			get
			{
				return (byte)((this.RawData[0] & 96) >> 5);
			}
			set
			{
				this.RawData[0] = (byte)((int)(this.RawData[0] & 159) | ((int)value << 5));
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x060000FD RID: 253 RVA: 0x000068DE File Offset: 0x00004ADE
		// (set) Token: 0x060000FE RID: 254 RVA: 0x000068EC File Offset: 0x00004AEC
		public ushort Sequence
		{
			get
			{
				return BitConverter.ToUInt16(this.RawData, 1);
			}
			set
			{
				FastBitConverter.GetBytes(this.RawData, 1, value);
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x060000FF RID: 255 RVA: 0x000068FB File Offset: 0x00004AFB
		public bool IsFragmented
		{
			get
			{
				return (this.RawData[0] & 128) > 0;
			}
		}

		// Token: 0x06000100 RID: 256 RVA: 0x0000690E File Offset: 0x00004B0E
		public void MarkFragmented()
		{
			byte[] rawData = this.RawData;
			int num = 0;
			rawData[num] |= 128;
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000101 RID: 257 RVA: 0x00006926 File Offset: 0x00004B26
		// (set) Token: 0x06000102 RID: 258 RVA: 0x00006930 File Offset: 0x00004B30
		public byte ChannelId
		{
			get
			{
				return this.RawData[3];
			}
			set
			{
				this.RawData[3] = value;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000103 RID: 259 RVA: 0x0000693B File Offset: 0x00004B3B
		// (set) Token: 0x06000104 RID: 260 RVA: 0x00006949 File Offset: 0x00004B49
		public ushort FragmentId
		{
			get
			{
				return BitConverter.ToUInt16(this.RawData, 4);
			}
			set
			{
				FastBitConverter.GetBytes(this.RawData, 4, value);
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000105 RID: 261 RVA: 0x00006958 File Offset: 0x00004B58
		// (set) Token: 0x06000106 RID: 262 RVA: 0x00006966 File Offset: 0x00004B66
		public ushort FragmentPart
		{
			get
			{
				return BitConverter.ToUInt16(this.RawData, 6);
			}
			set
			{
				FastBitConverter.GetBytes(this.RawData, 6, value);
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000107 RID: 263 RVA: 0x00006975 File Offset: 0x00004B75
		// (set) Token: 0x06000108 RID: 264 RVA: 0x00006983 File Offset: 0x00004B83
		public ushort FragmentsTotal
		{
			get
			{
				return BitConverter.ToUInt16(this.RawData, 8);
			}
			set
			{
				FastBitConverter.GetBytes(this.RawData, 8, value);
			}
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00006992 File Offset: 0x00004B92
		public NetPacket(int size)
		{
			this.RawData = new byte[size];
			this.Size = size;
		}

		// Token: 0x0600010A RID: 266 RVA: 0x000069AD File Offset: 0x00004BAD
		public NetPacket(PacketProperty property, int size)
		{
			size += NetPacket.GetHeaderSize(property);
			this.RawData = new byte[size];
			this.Property = property;
			this.Size = size;
		}

		// Token: 0x0600010B RID: 267 RVA: 0x000069D9 File Offset: 0x00004BD9
		public static int GetHeaderSize(PacketProperty property)
		{
			return NetPacket.HeaderSizes[(int)property];
		}

		// Token: 0x0600010C RID: 268 RVA: 0x000069E2 File Offset: 0x00004BE2
		public int GetHeaderSize()
		{
			return NetPacket.HeaderSizes[(int)(this.RawData[0] & 31)];
		}

		// Token: 0x0600010D RID: 269 RVA: 0x000069F8 File Offset: 0x00004BF8
		public bool Verify()
		{
			byte b = this.RawData[0] & 31;
			if ((int)b >= NetPacket.PropertiesCount)
			{
				return false;
			}
			int num = NetPacket.HeaderSizes[(int)b];
			bool flag = (this.RawData[0] & 128) > 0;
			return this.Size >= num && (!flag || this.Size >= num + 6);
		}

		// Token: 0x040000E3 RID: 227
		private static readonly int PropertiesCount = Enum.GetValues(typeof(PacketProperty)).Length;

		// Token: 0x040000E4 RID: 228
		private static readonly int[] HeaderSizes = NetUtils.AllocatePinnedUninitializedArray<int>(NetPacket.PropertiesCount);

		// Token: 0x040000E5 RID: 229
		public byte[] RawData;

		// Token: 0x040000E6 RID: 230
		public int Size;

		// Token: 0x040000E7 RID: 231
		public object UserData;

		// Token: 0x040000E8 RID: 232
		public NetPacket Next;
	}
}
