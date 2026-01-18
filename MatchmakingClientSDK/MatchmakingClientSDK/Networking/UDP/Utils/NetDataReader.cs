using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace Networking.UDP.Utils
{
	// Token: 0x0200003A RID: 58
	public class NetDataReader
	{
		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000193 RID: 403 RVA: 0x00009B7D File Offset: 0x00007D7D
		public byte[] RawData
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._data;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000194 RID: 404 RVA: 0x00009B85 File Offset: 0x00007D85
		public int RawDataSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._dataSize;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000195 RID: 405 RVA: 0x00009B8D File Offset: 0x00007D8D
		public int UserDataOffset
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._offset;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000196 RID: 406 RVA: 0x00009B95 File Offset: 0x00007D95
		public int UserDataSize
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._dataSize - this._offset;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000197 RID: 407 RVA: 0x00009BA4 File Offset: 0x00007DA4
		public bool IsNull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._data == null;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000198 RID: 408 RVA: 0x00009BAF File Offset: 0x00007DAF
		public int Position
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._position;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000199 RID: 409 RVA: 0x00009BB7 File Offset: 0x00007DB7
		public bool EndOfData
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._position == this._dataSize;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600019A RID: 410 RVA: 0x00009BC7 File Offset: 0x00007DC7
		public int AvailableBytes
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this._dataSize - this._position;
			}
		}

		// Token: 0x0600019B RID: 411 RVA: 0x00009BD6 File Offset: 0x00007DD6
		public void SkipBytes(int count)
		{
			this._position += count;
		}

		// Token: 0x0600019C RID: 412 RVA: 0x00009BE6 File Offset: 0x00007DE6
		public void SetPosition(int position)
		{
			this._position = position;
		}

		// Token: 0x0600019D RID: 413 RVA: 0x00009BEF File Offset: 0x00007DEF
		public void SetSource(NetDataWriter dataWriter)
		{
			this._data = dataWriter.Data;
			this._position = 0;
			this._offset = 0;
			this._dataSize = dataWriter.Length;
		}

		// Token: 0x0600019E RID: 414 RVA: 0x00009C17 File Offset: 0x00007E17
		public void SetSource(byte[] source)
		{
			this._data = source;
			this._position = 0;
			this._offset = 0;
			this._dataSize = source.Length;
		}

		// Token: 0x0600019F RID: 415 RVA: 0x00009C37 File Offset: 0x00007E37
		public void SetSource(byte[] source, int offset, int maxSize)
		{
			this._data = source;
			this._position = offset;
			this._offset = offset;
			this._dataSize = maxSize;
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x00009C55 File Offset: 0x00007E55
		public NetDataReader()
		{
		}

		// Token: 0x060001A1 RID: 417 RVA: 0x00009C5D File Offset: 0x00007E5D
		public NetDataReader(NetDataWriter writer)
		{
			this.SetSource(writer);
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x00009C6C File Offset: 0x00007E6C
		public NetDataReader(byte[] source)
		{
			this.SetSource(source);
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x00009C7B File Offset: 0x00007E7B
		public NetDataReader(byte[] source, int offset, int maxSize)
		{
			this.SetSource(source, offset, maxSize);
		}

		// Token: 0x060001A4 RID: 420 RVA: 0x00009C8C File Offset: 0x00007E8C
		public void Get<T>(out T result) where T : struct, INetSerializable
		{
			result = default(T);
			result.Deserialize(this);
		}

		// Token: 0x060001A5 RID: 421 RVA: 0x00009CA2 File Offset: 0x00007EA2
		public void Get<T>(out T result, Func<T> constructor) where T : class, INetSerializable
		{
			result = constructor();
			result.Deserialize(this);
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x00009CBD File Offset: 0x00007EBD
		public void Get(out IPEndPoint result)
		{
			result = this.GetNetEndPoint();
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x00009CC7 File Offset: 0x00007EC7
		public void Get(out byte result)
		{
			result = this.GetByte();
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x00009CD1 File Offset: 0x00007ED1
		public void Get(out sbyte result)
		{
			result = (sbyte)this.GetByte();
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x00009CDC File Offset: 0x00007EDC
		public void Get(out bool result)
		{
			result = this.GetBool();
		}

		// Token: 0x060001AA RID: 426 RVA: 0x00009CE6 File Offset: 0x00007EE6
		public void Get(out char result)
		{
			result = this.GetChar();
		}

		// Token: 0x060001AB RID: 427 RVA: 0x00009CF0 File Offset: 0x00007EF0
		public void Get(out ushort result)
		{
			result = this.GetUShort();
		}

		// Token: 0x060001AC RID: 428 RVA: 0x00009CFA File Offset: 0x00007EFA
		public void Get(out short result)
		{
			result = this.GetShort();
		}

		// Token: 0x060001AD RID: 429 RVA: 0x00009D04 File Offset: 0x00007F04
		public void Get(out ulong result)
		{
			result = this.GetULong();
		}

		// Token: 0x060001AE RID: 430 RVA: 0x00009D0E File Offset: 0x00007F0E
		public void Get(out long result)
		{
			result = this.GetLong();
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00009D18 File Offset: 0x00007F18
		public void Get(out uint result)
		{
			result = this.GetUInt();
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x00009D22 File Offset: 0x00007F22
		public void Get(out int result)
		{
			result = this.GetInt();
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00009D2C File Offset: 0x00007F2C
		public void Get(out double result)
		{
			result = this.GetDouble();
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00009D36 File Offset: 0x00007F36
		public void Get(out float result)
		{
			result = this.GetFloat();
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00009D40 File Offset: 0x00007F40
		public void Get(out string result)
		{
			result = this.GetString();
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00009D4A File Offset: 0x00007F4A
		public void Get(out string result, int maxLength)
		{
			result = this.GetString(maxLength);
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x00009D55 File Offset: 0x00007F55
		public void Get(out Guid result)
		{
			result = this.GetGuid();
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x00009D64 File Offset: 0x00007F64
		public IPEndPoint GetNetEndPoint()
		{
			string @string = this.GetString(1000);
			int @int = this.GetInt();
			return NetUtils.MakeEndPoint(@string, @int);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00009D89 File Offset: 0x00007F89
		public byte GetByte()
		{
			byte b = this._data[this._position];
			this._position++;
			return b;
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00009DA6 File Offset: 0x00007FA6
		public sbyte GetSByte()
		{
			return (sbyte)this.GetByte();
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x00009DB0 File Offset: 0x00007FB0
		public T[] GetArray<T>(ushort size)
		{
			ushort num = BitConverter.ToUInt16(this._data, this._position);
			this._position += 2;
			T[] array = new T[(int)num];
			num *= size;
			Buffer.BlockCopy(this._data, this._position, array, 0, (int)num);
			this._position += (int)num;
			return array;
		}

		// Token: 0x060001BA RID: 442 RVA: 0x00009E0C File Offset: 0x0000800C
		public T[] GetArray<T>() where T : INetSerializable, new()
		{
			ushort num = BitConverter.ToUInt16(this._data, this._position);
			this._position += 2;
			T[] array = new T[(int)num];
			for (int i = 0; i < (int)num; i++)
			{
				T t = new T();
				t.Deserialize(this);
				array[i] = t;
			}
			return array;
		}

		// Token: 0x060001BB RID: 443 RVA: 0x00009E6C File Offset: 0x0000806C
		public T[] GetArray<T>(Func<T> constructor) where T : class, INetSerializable
		{
			ushort num = BitConverter.ToUInt16(this._data, this._position);
			this._position += 2;
			T[] array = new T[(int)num];
			for (int i = 0; i < (int)num; i++)
			{
				this.Get<T>(out array[i], constructor);
			}
			return array;
		}

		// Token: 0x060001BC RID: 444 RVA: 0x00009EBB File Offset: 0x000080BB
		public bool[] GetBoolArray()
		{
			return this.GetArray<bool>(1);
		}

		// Token: 0x060001BD RID: 445 RVA: 0x00009EC4 File Offset: 0x000080C4
		public ushort[] GetUShortArray()
		{
			return this.GetArray<ushort>(2);
		}

		// Token: 0x060001BE RID: 446 RVA: 0x00009ECD File Offset: 0x000080CD
		public short[] GetShortArray()
		{
			return this.GetArray<short>(2);
		}

		// Token: 0x060001BF RID: 447 RVA: 0x00009ED6 File Offset: 0x000080D6
		public int[] GetIntArray()
		{
			return this.GetArray<int>(4);
		}

		// Token: 0x060001C0 RID: 448 RVA: 0x00009EDF File Offset: 0x000080DF
		public uint[] GetUIntArray()
		{
			return this.GetArray<uint>(4);
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x00009EE8 File Offset: 0x000080E8
		public float[] GetFloatArray()
		{
			return this.GetArray<float>(4);
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x00009EF1 File Offset: 0x000080F1
		public double[] GetDoubleArray()
		{
			return this.GetArray<double>(8);
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x00009EFA File Offset: 0x000080FA
		public long[] GetLongArray()
		{
			return this.GetArray<long>(8);
		}

		// Token: 0x060001C4 RID: 452 RVA: 0x00009F03 File Offset: 0x00008103
		public ulong[] GetULongArray()
		{
			return this.GetArray<ulong>(8);
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x00009F0C File Offset: 0x0000810C
		public string[] GetStringArray()
		{
			ushort @ushort = this.GetUShort();
			string[] array = new string[(int)@ushort];
			for (int i = 0; i < (int)@ushort; i++)
			{
				array[i] = this.GetString();
			}
			return array;
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x00009F40 File Offset: 0x00008140
		public string[] GetStringArray(int maxStringLength)
		{
			ushort @ushort = this.GetUShort();
			string[] array = new string[(int)@ushort];
			for (int i = 0; i < (int)@ushort; i++)
			{
				array[i] = this.GetString(maxStringLength);
			}
			return array;
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x00009F72 File Offset: 0x00008172
		public bool GetBool()
		{
			return this.GetByte() == 1;
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x00009F7D File Offset: 0x0000817D
		public char GetChar()
		{
			return (char)this.GetUShort();
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x00009F85 File Offset: 0x00008185
		public ushort GetUShort()
		{
			ushort num = BitConverter.ToUInt16(this._data, this._position);
			this._position += 2;
			return num;
		}

		// Token: 0x060001CA RID: 458 RVA: 0x00009FA6 File Offset: 0x000081A6
		public short GetShort()
		{
			short num = BitConverter.ToInt16(this._data, this._position);
			this._position += 2;
			return num;
		}

		// Token: 0x060001CB RID: 459 RVA: 0x00009FC7 File Offset: 0x000081C7
		public long GetLong()
		{
			long num = BitConverter.ToInt64(this._data, this._position);
			this._position += 8;
			return num;
		}

		// Token: 0x060001CC RID: 460 RVA: 0x00009FE8 File Offset: 0x000081E8
		public ulong GetULong()
		{
			ulong num = BitConverter.ToUInt64(this._data, this._position);
			this._position += 8;
			return num;
		}

		// Token: 0x060001CD RID: 461 RVA: 0x0000A009 File Offset: 0x00008209
		public int GetInt()
		{
			int num = BitConverter.ToInt32(this._data, this._position);
			this._position += 4;
			return num;
		}

		// Token: 0x060001CE RID: 462 RVA: 0x0000A02A File Offset: 0x0000822A
		public uint GetUInt()
		{
			uint num = BitConverter.ToUInt32(this._data, this._position);
			this._position += 4;
			return num;
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000A04B File Offset: 0x0000824B
		public float GetFloat()
		{
			float num = BitConverter.ToSingle(this._data, this._position);
			this._position += 4;
			return num;
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x0000A06C File Offset: 0x0000826C
		public double GetDouble()
		{
			double num = BitConverter.ToDouble(this._data, this._position);
			this._position += 8;
			return num;
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x0000A090 File Offset: 0x00008290
		public string GetString(int maxLength)
		{
			ushort @ushort = this.GetUShort();
			if (@ushort == 0)
			{
				return string.Empty;
			}
			int num = (int)(@ushort - 1);
			string text = ((maxLength > 0 && NetDataWriter.uTF8Encoding.Value.GetCharCount(this._data, this._position, num) > maxLength) ? string.Empty : NetDataWriter.uTF8Encoding.Value.GetString(this._data, this._position, num));
			this._position += num;
			return text;
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x0000A108 File Offset: 0x00008308
		public string GetString()
		{
			ushort @ushort = this.GetUShort();
			if (@ushort == 0)
			{
				return string.Empty;
			}
			int num = (int)(@ushort - 1);
			string @string = NetDataWriter.uTF8Encoding.Value.GetString(this._data, this._position, num);
			this._position += num;
			return @string;
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x0000A154 File Offset: 0x00008354
		public string GetLargeString()
		{
			int @int = this.GetInt();
			if (@int <= 0)
			{
				return string.Empty;
			}
			string @string = NetDataWriter.uTF8Encoding.Value.GetString(this._data, this._position, @int);
			this._position += @int;
			return @string;
		}

		// Token: 0x060001D4 RID: 468 RVA: 0x0000A19C File Offset: 0x0000839C
		public Guid GetGuid()
		{
			return new Guid(this.GetBytesWithLength());
		}

		// Token: 0x060001D5 RID: 469 RVA: 0x0000A1A9 File Offset: 0x000083A9
		public ArraySegment<byte> GetBytesSegment(int count)
		{
			ArraySegment<byte> arraySegment = new ArraySegment<byte>(this._data, this._position, count);
			this._position += count;
			return arraySegment;
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x0000A1CB File Offset: 0x000083CB
		public ArraySegment<byte> GetRemainingBytesSegment()
		{
			ArraySegment<byte> arraySegment = new ArraySegment<byte>(this._data, this._position, this.AvailableBytes);
			this._position = this._data.Length;
			return arraySegment;
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x0000A1F4 File Offset: 0x000083F4
		public T Get<T>() where T : struct, INetSerializable
		{
			T t = default(T);
			t.Deserialize(this);
			return t;
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x0000A218 File Offset: 0x00008418
		public T Get<T>(Func<T> constructor) where T : class, INetSerializable
		{
			T t = constructor();
			t.Deserialize(this);
			return t;
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x0000A22C File Offset: 0x0000842C
		public byte[] GetRemainingBytes()
		{
			byte[] array = new byte[this.AvailableBytes];
			Buffer.BlockCopy(this._data, this._position, array, 0, this.AvailableBytes);
			this._position = this._data.Length;
			return array;
		}

		// Token: 0x060001DA RID: 474 RVA: 0x0000A26D File Offset: 0x0000846D
		public void GetBytes(byte[] destination, int start, int count)
		{
			Buffer.BlockCopy(this._data, this._position, destination, start, count);
			this._position += count;
		}

		// Token: 0x060001DB RID: 475 RVA: 0x0000A291 File Offset: 0x00008491
		public void GetBytes(byte[] destination, int count)
		{
			Buffer.BlockCopy(this._data, this._position, destination, 0, count);
			this._position += count;
		}

		// Token: 0x060001DC RID: 476 RVA: 0x0000A2B5 File Offset: 0x000084B5
		public sbyte[] GetSBytesWithLength()
		{
			return this.GetArray<sbyte>(1);
		}

		// Token: 0x060001DD RID: 477 RVA: 0x0000A2BE File Offset: 0x000084BE
		public byte[] GetBytesWithLength()
		{
			return this.GetArray<byte>(1);
		}

		// Token: 0x060001DE RID: 478 RVA: 0x0000A2C7 File Offset: 0x000084C7
		public byte PeekByte()
		{
			return this._data[this._position];
		}

		// Token: 0x060001DF RID: 479 RVA: 0x0000A2D6 File Offset: 0x000084D6
		public sbyte PeekSByte()
		{
			return (sbyte)this._data[this._position];
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000A2E6 File Offset: 0x000084E6
		public bool PeekBool()
		{
			return this._data[this._position] == 1;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x0000A2F8 File Offset: 0x000084F8
		public char PeekChar()
		{
			return (char)this.PeekUShort();
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x0000A300 File Offset: 0x00008500
		public ushort PeekUShort()
		{
			return BitConverter.ToUInt16(this._data, this._position);
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x0000A313 File Offset: 0x00008513
		public short PeekShort()
		{
			return BitConverter.ToInt16(this._data, this._position);
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x0000A326 File Offset: 0x00008526
		public long PeekLong()
		{
			return BitConverter.ToInt64(this._data, this._position);
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x0000A339 File Offset: 0x00008539
		public ulong PeekULong()
		{
			return BitConverter.ToUInt64(this._data, this._position);
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x0000A34C File Offset: 0x0000854C
		public int PeekInt()
		{
			return BitConverter.ToInt32(this._data, this._position);
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x0000A35F File Offset: 0x0000855F
		public uint PeekUInt()
		{
			return BitConverter.ToUInt32(this._data, this._position);
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0000A372 File Offset: 0x00008572
		public float PeekFloat()
		{
			return BitConverter.ToSingle(this._data, this._position);
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000A385 File Offset: 0x00008585
		public double PeekDouble()
		{
			return BitConverter.ToDouble(this._data, this._position);
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000A398 File Offset: 0x00008598
		public string PeekString(int maxLength)
		{
			ushort num = this.PeekUShort();
			if (num == 0)
			{
				return string.Empty;
			}
			int num2 = (int)(num - 1);
			if (maxLength <= 0 || NetDataWriter.uTF8Encoding.Value.GetCharCount(this._data, this._position + 2, num2) <= maxLength)
			{
				return NetDataWriter.uTF8Encoding.Value.GetString(this._data, this._position + 2, num2);
			}
			return string.Empty;
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0000A404 File Offset: 0x00008604
		public string PeekString()
		{
			ushort num = this.PeekUShort();
			if (num == 0)
			{
				return string.Empty;
			}
			int num2 = (int)(num - 1);
			return NetDataWriter.uTF8Encoding.Value.GetString(this._data, this._position + 2, num2);
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0000A443 File Offset: 0x00008643
		public bool TryGetByte(out byte result)
		{
			if (this.AvailableBytes >= 1)
			{
				result = this.GetByte();
				return true;
			}
			result = 0;
			return false;
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0000A45C File Offset: 0x0000865C
		public bool TryGetSByte(out sbyte result)
		{
			if (this.AvailableBytes >= 1)
			{
				result = this.GetSByte();
				return true;
			}
			result = 0;
			return false;
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000A475 File Offset: 0x00008675
		public bool TryGetBool(out bool result)
		{
			if (this.AvailableBytes >= 1)
			{
				result = this.GetBool();
				return true;
			}
			result = false;
			return false;
		}

		// Token: 0x060001EF RID: 495 RVA: 0x0000A490 File Offset: 0x00008690
		public bool TryGetChar(out char result)
		{
			ushort num;
			if (!this.TryGetUShort(out num))
			{
				result = '\0';
				return false;
			}
			result = (char)num;
			return true;
		}

		// Token: 0x060001F0 RID: 496 RVA: 0x0000A4B0 File Offset: 0x000086B0
		public bool TryGetShort(out short result)
		{
			if (this.AvailableBytes >= 2)
			{
				result = this.GetShort();
				return true;
			}
			result = 0;
			return false;
		}

		// Token: 0x060001F1 RID: 497 RVA: 0x0000A4C9 File Offset: 0x000086C9
		public bool TryGetUShort(out ushort result)
		{
			if (this.AvailableBytes >= 2)
			{
				result = this.GetUShort();
				return true;
			}
			result = 0;
			return false;
		}

		// Token: 0x060001F2 RID: 498 RVA: 0x0000A4E2 File Offset: 0x000086E2
		public bool TryGetInt(out int result)
		{
			if (this.AvailableBytes >= 4)
			{
				result = this.GetInt();
				return true;
			}
			result = 0;
			return false;
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x0000A4FB File Offset: 0x000086FB
		public bool TryGetUInt(out uint result)
		{
			if (this.AvailableBytes >= 4)
			{
				result = this.GetUInt();
				return true;
			}
			result = 0U;
			return false;
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x0000A514 File Offset: 0x00008714
		public bool TryGetLong(out long result)
		{
			if (this.AvailableBytes >= 8)
			{
				result = this.GetLong();
				return true;
			}
			result = 0L;
			return false;
		}

		// Token: 0x060001F5 RID: 501 RVA: 0x0000A52E File Offset: 0x0000872E
		public bool TryGetULong(out ulong result)
		{
			if (this.AvailableBytes >= 8)
			{
				result = this.GetULong();
				return true;
			}
			result = 0UL;
			return false;
		}

		// Token: 0x060001F6 RID: 502 RVA: 0x0000A548 File Offset: 0x00008748
		public bool TryGetFloat(out float result)
		{
			if (this.AvailableBytes >= 4)
			{
				result = this.GetFloat();
				return true;
			}
			result = 0f;
			return false;
		}

		// Token: 0x060001F7 RID: 503 RVA: 0x0000A565 File Offset: 0x00008765
		public bool TryGetDouble(out double result)
		{
			if (this.AvailableBytes >= 8)
			{
				result = this.GetDouble();
				return true;
			}
			result = 0.0;
			return false;
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x0000A588 File Offset: 0x00008788
		public bool TryGetString(out string result)
		{
			if (this.AvailableBytes >= 2)
			{
				ushort num = this.PeekUShort();
				if (this.AvailableBytes >= (int)(num + 1))
				{
					result = this.GetString();
					return true;
				}
			}
			result = null;
			return false;
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x0000A5C0 File Offset: 0x000087C0
		public bool TryGetStringArray(out string[] result)
		{
			ushort num;
			if (!this.TryGetUShort(out num))
			{
				result = null;
				return false;
			}
			result = new string[(int)num];
			for (int i = 0; i < (int)num; i++)
			{
				if (!this.TryGetString(out result[i]))
				{
					result = null;
					return false;
				}
			}
			return true;
		}

		// Token: 0x060001FA RID: 506 RVA: 0x0000A608 File Offset: 0x00008808
		public bool TryGetBytesWithLength(out byte[] result)
		{
			if (this.AvailableBytes >= 2)
			{
				ushort num = this.PeekUShort();
				if (num >= 0 && this.AvailableBytes >= (int)(2 + num))
				{
					result = this.GetBytesWithLength();
					return true;
				}
			}
			result = null;
			return false;
		}

		// Token: 0x060001FB RID: 507 RVA: 0x0000A642 File Offset: 0x00008842
		public void Clear()
		{
			this._position = 0;
			this._dataSize = 0;
			this._data = null;
		}

		// Token: 0x0400016B RID: 363
		protected byte[] _data;

		// Token: 0x0400016C RID: 364
		protected int _position;

		// Token: 0x0400016D RID: 365
		protected int _dataSize;

		// Token: 0x0400016E RID: 366
		private int _offset;
	}
}
