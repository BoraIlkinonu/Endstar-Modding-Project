using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Networking.UDP.Utils
{
	// Token: 0x0200003B RID: 59
	public class NetDataWriter
	{
		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060001FC RID: 508 RVA: 0x0000A659 File Offset: 0x00008859
		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._data.Length;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060001FD RID: 509 RVA: 0x0000A663 File Offset: 0x00008863
		public byte[] Data
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._data;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060001FE RID: 510 RVA: 0x0000A66B File Offset: 0x0000886B
		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._position;
			}
		}

		// Token: 0x060001FF RID: 511 RVA: 0x0000A673 File Offset: 0x00008873
		public NetDataWriter()
			: this(true, 64)
		{
		}

		// Token: 0x06000200 RID: 512 RVA: 0x0000A67E File Offset: 0x0000887E
		public NetDataWriter(bool autoResize)
			: this(autoResize, 64)
		{
		}

		// Token: 0x06000201 RID: 513 RVA: 0x0000A689 File Offset: 0x00008889
		public NetDataWriter(bool autoResize, int initialSize)
		{
			this._data = new byte[initialSize];
			this._autoResize = autoResize;
		}

		// Token: 0x06000202 RID: 514 RVA: 0x0000A6A4 File Offset: 0x000088A4
		public static NetDataWriter FromBytes(byte[] bytes, bool copy)
		{
			if (copy)
			{
				NetDataWriter netDataWriter = new NetDataWriter(true, bytes.Length);
				netDataWriter.Put(bytes);
				return netDataWriter;
			}
			return new NetDataWriter(true, 0)
			{
				_data = bytes,
				_position = bytes.Length
			};
		}

		// Token: 0x06000203 RID: 515 RVA: 0x0000A6D1 File Offset: 0x000088D1
		public static NetDataWriter FromBytes(byte[] bytes, int offset, int length)
		{
			NetDataWriter netDataWriter = new NetDataWriter(true, bytes.Length);
			netDataWriter.Put(bytes, offset, length);
			return netDataWriter;
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000A6E5 File Offset: 0x000088E5
		public static NetDataWriter FromString(string value)
		{
			NetDataWriter netDataWriter = new NetDataWriter();
			netDataWriter.Put(value);
			return netDataWriter;
		}

		// Token: 0x06000205 RID: 517 RVA: 0x0000A6F3 File Offset: 0x000088F3
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResizeIfNeed(int newSize)
		{
			if (this._data.Length < newSize)
			{
				Array.Resize<byte>(ref this._data, Math.Max(newSize, this._data.Length * 2));
			}
		}

		// Token: 0x06000206 RID: 518 RVA: 0x0000A71B File Offset: 0x0000891B
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureFit(int additionalSize)
		{
			if (this._data.Length < this._position + additionalSize)
			{
				Array.Resize<byte>(ref this._data, Math.Max(this._position + additionalSize, this._data.Length * 2));
			}
		}

		// Token: 0x06000207 RID: 519 RVA: 0x0000A751 File Offset: 0x00008951
		public void Reset(int size)
		{
			this.ResizeIfNeed(size);
			this._position = 0;
		}

		// Token: 0x06000208 RID: 520 RVA: 0x0000A761 File Offset: 0x00008961
		public void Reset()
		{
			this._position = 0;
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000A76C File Offset: 0x0000896C
		public byte[] CopyData()
		{
			byte[] array = new byte[this._position];
			Buffer.BlockCopy(this._data, 0, array, 0, this._position);
			return array;
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000A79A File Offset: 0x0000899A
		public int SetPosition(int position)
		{
			int position2 = this._position;
			this._position = position;
			return position2;
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000A7A9 File Offset: 0x000089A9
		public void Put(float value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 4);
			}
			FastBitConverter.GetBytes(this._data, this._position, value);
			this._position += 4;
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000A7E1 File Offset: 0x000089E1
		public void Put(double value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 8);
			}
			FastBitConverter.GetBytes(this._data, this._position, value);
			this._position += 8;
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0000A819 File Offset: 0x00008A19
		public void Put(long value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 8);
			}
			FastBitConverter.GetBytes(this._data, this._position, value);
			this._position += 8;
		}

		// Token: 0x0600020E RID: 526 RVA: 0x0000A851 File Offset: 0x00008A51
		public void Put(ulong value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 8);
			}
			FastBitConverter.GetBytes(this._data, this._position, value);
			this._position += 8;
		}

		// Token: 0x0600020F RID: 527 RVA: 0x0000A889 File Offset: 0x00008A89
		public void Put(int value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 4);
			}
			FastBitConverter.GetBytes(this._data, this._position, value);
			this._position += 4;
		}

		// Token: 0x06000210 RID: 528 RVA: 0x0000A8C1 File Offset: 0x00008AC1
		public void Put(uint value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 4);
			}
			FastBitConverter.GetBytes(this._data, this._position, value);
			this._position += 4;
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0000A8F9 File Offset: 0x00008AF9
		public void Put(char value)
		{
			this.Put((ushort)value);
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0000A902 File Offset: 0x00008B02
		public void Put(ushort value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 2);
			}
			FastBitConverter.GetBytes(this._data, this._position, value);
			this._position += 2;
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0000A93A File Offset: 0x00008B3A
		public void Put(short value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 2);
			}
			FastBitConverter.GetBytes(this._data, this._position, value);
			this._position += 2;
		}

		// Token: 0x06000214 RID: 532 RVA: 0x0000A972 File Offset: 0x00008B72
		public void Put(sbyte value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 1);
			}
			this._data[this._position] = (byte)value;
			this._position++;
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0000A9A7 File Offset: 0x00008BA7
		public void Put(byte value)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 1);
			}
			this._data[this._position] = value;
			this._position++;
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000A9DB File Offset: 0x00008BDB
		public void Put(Guid value)
		{
			this.PutBytesWithLength(value.ToByteArray());
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0000A9EA File Offset: 0x00008BEA
		public void Put(byte[] data, int offset, int length)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + length);
			}
			Buffer.BlockCopy(data, offset, this._data, this._position, length);
			this._position += length;
		}

		// Token: 0x06000218 RID: 536 RVA: 0x0000AA24 File Offset: 0x00008C24
		public void Put(byte[] data)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + data.Length);
			}
			Buffer.BlockCopy(data, 0, this._data, this._position, data.Length);
			this._position += data.Length;
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0000AA64 File Offset: 0x00008C64
		public void PutSBytesWithLength(sbyte[] data, int offset, ushort length)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 2 + (int)length);
			}
			FastBitConverter.GetBytes(this._data, this._position, length);
			Buffer.BlockCopy(data, offset, this._data, this._position + 2, (int)length);
			this._position += (int)(2 + length);
		}

		// Token: 0x0600021A RID: 538 RVA: 0x0000AAC1 File Offset: 0x00008CC1
		public void PutSBytesWithLength(sbyte[] data)
		{
			this.PutArray(data, 1);
		}

		// Token: 0x0600021B RID: 539 RVA: 0x0000AACC File Offset: 0x00008CCC
		public void PutBytesWithLength(byte[] data, int offset, ushort length)
		{
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + 2 + (int)length);
			}
			FastBitConverter.GetBytes(this._data, this._position, length);
			Buffer.BlockCopy(data, offset, this._data, this._position + 2, (int)length);
			this._position += (int)(2 + length);
		}

		// Token: 0x0600021C RID: 540 RVA: 0x0000AB29 File Offset: 0x00008D29
		public void PutBytesWithLength(byte[] data)
		{
			this.PutArray(data, 1);
		}

		// Token: 0x0600021D RID: 541 RVA: 0x0000AB33 File Offset: 0x00008D33
		public void Put(bool value)
		{
			this.Put((value > false) ? 1 : 0);
		}

		// Token: 0x0600021E RID: 542 RVA: 0x0000AB40 File Offset: 0x00008D40
		public void PutArray(Array arr, int sz)
		{
			ushort num = ((arr == null) ? 0 : ((ushort)arr.Length));
			sz *= (int)num;
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + sz + 2);
			}
			FastBitConverter.GetBytes(this._data, this._position, num);
			if (arr != null)
			{
				Buffer.BlockCopy(arr, 0, this._data, this._position + 2, sz);
			}
			this._position += sz + 2;
		}

		// Token: 0x0600021F RID: 543 RVA: 0x0000ABB3 File Offset: 0x00008DB3
		public void PutArray(float[] value)
		{
			this.PutArray(value, 4);
		}

		// Token: 0x06000220 RID: 544 RVA: 0x0000ABBD File Offset: 0x00008DBD
		public void PutArray(double[] value)
		{
			this.PutArray(value, 8);
		}

		// Token: 0x06000221 RID: 545 RVA: 0x0000ABC7 File Offset: 0x00008DC7
		public void PutArray(long[] value)
		{
			this.PutArray(value, 8);
		}

		// Token: 0x06000222 RID: 546 RVA: 0x0000ABD1 File Offset: 0x00008DD1
		public void PutArray(ulong[] value)
		{
			this.PutArray(value, 8);
		}

		// Token: 0x06000223 RID: 547 RVA: 0x0000ABDB File Offset: 0x00008DDB
		public void PutArray(int[] value)
		{
			this.PutArray(value, 4);
		}

		// Token: 0x06000224 RID: 548 RVA: 0x0000ABE5 File Offset: 0x00008DE5
		public void PutArray(uint[] value)
		{
			this.PutArray(value, 4);
		}

		// Token: 0x06000225 RID: 549 RVA: 0x0000ABEF File Offset: 0x00008DEF
		public void PutArray(ushort[] value)
		{
			this.PutArray(value, 2);
		}

		// Token: 0x06000226 RID: 550 RVA: 0x0000ABF9 File Offset: 0x00008DF9
		public void PutArray(short[] value)
		{
			this.PutArray(value, 2);
		}

		// Token: 0x06000227 RID: 551 RVA: 0x0000AC03 File Offset: 0x00008E03
		public void PutArray(bool[] value)
		{
			this.PutArray(value, 1);
		}

		// Token: 0x06000228 RID: 552 RVA: 0x0000AC10 File Offset: 0x00008E10
		public void PutArray(string[] value)
		{
			ushort num = ((value == null) ? 0 : ((ushort)value.Length));
			this.Put(num);
			for (int i = 0; i < (int)num; i++)
			{
				this.Put(value[i]);
			}
		}

		// Token: 0x06000229 RID: 553 RVA: 0x0000AC44 File Offset: 0x00008E44
		public void PutArray(string[] value, int strMaxLength)
		{
			ushort num = ((value == null) ? 0 : ((ushort)value.Length));
			this.Put(num);
			for (int i = 0; i < (int)num; i++)
			{
				this.Put(value[i], strMaxLength);
			}
		}

		// Token: 0x0600022A RID: 554 RVA: 0x0000AC7C File Offset: 0x00008E7C
		public void PutArray<T>(T[] value) where T : INetSerializable, new()
		{
			ushort num = (ushort)((value != null) ? value.Length : 0);
			this.Put(num);
			for (int i = 0; i < (int)num; i++)
			{
				value[i].Serialize(this);
			}
		}

		// Token: 0x0600022B RID: 555 RVA: 0x0000ACBC File Offset: 0x00008EBC
		public void Put(IPEndPoint endPoint)
		{
			this.Put(endPoint.Address.ToString());
			this.Put(endPoint.Port);
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0000ACDC File Offset: 0x00008EDC
		public void PutLargeString(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				this.Put(0);
				return;
			}
			int byteCount = NetDataWriter.uTF8Encoding.Value.GetByteCount(value);
			if (byteCount == 0)
			{
				this.Put(0);
				return;
			}
			this.Put(byteCount);
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + byteCount);
			}
			NetDataWriter.uTF8Encoding.Value.GetBytes(value, 0, byteCount, this._data, this._position);
			this._position += byteCount;
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0000AD5F File Offset: 0x00008F5F
		public void Put(string value)
		{
			this.Put(value, 0);
		}

		// Token: 0x0600022E RID: 558 RVA: 0x0000AD6C File Offset: 0x00008F6C
		public void Put(string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value))
			{
				this.Put(0);
				return;
			}
			int num = ((maxLength > 0 && value.Length > maxLength) ? maxLength : value.Length);
			int maxByteCount = NetDataWriter.uTF8Encoding.Value.GetMaxByteCount(num);
			if (this._autoResize)
			{
				this.ResizeIfNeed(this._position + maxByteCount + 2);
			}
			int bytes = NetDataWriter.uTF8Encoding.Value.GetBytes(value, 0, num, this._data, this._position + 2);
			if (bytes == 0)
			{
				this.Put(0);
				return;
			}
			this.Put(checked((ushort)(bytes + 1)));
			this._position += bytes;
		}

		// Token: 0x0600022F RID: 559 RVA: 0x0000AE0D File Offset: 0x0000900D
		public void Put<T>(T obj) where T : INetSerializable
		{
			obj.Serialize(this);
		}

		// Token: 0x0400016F RID: 367
		protected byte[] _data;

		// Token: 0x04000170 RID: 368
		protected int _position;

		// Token: 0x04000171 RID: 369
		private const int InitialSize = 64;

		// Token: 0x04000172 RID: 370
		private readonly bool _autoResize;

		// Token: 0x04000173 RID: 371
		public static readonly ThreadLocal<UTF8Encoding> uTF8Encoding = new ThreadLocal<UTF8Encoding>(() => new UTF8Encoding(false, true));
	}
}
