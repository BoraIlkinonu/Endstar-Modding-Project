using System;
using System.Collections.Generic;
using System.Text;

namespace Endless.Networking
{
	// Token: 0x02000006 RID: 6
	public sealed class DataBuffer : IDisposable
	{
		// Token: 0x06000013 RID: 19 RVA: 0x00002320 File Offset: 0x00000520
		public DataBuffer(int _initalSize = 1000)
		{
			this.bufferList = new List<byte>(_initalSize);
			this.readPosition = 0;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x0000234C File Offset: 0x0000054C
		public int GetReadPosition()
		{
			return this.readPosition;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002364 File Offset: 0x00000564
		public byte[] ToArray()
		{
			return this.bufferList.ToArray();
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002384 File Offset: 0x00000584
		public int Count()
		{
			return this.bufferList.Count;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000023A4 File Offset: 0x000005A4
		public int Length()
		{
			return this.Count() - this.readPosition;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000023C3 File Offset: 0x000005C3
		public void Clear()
		{
			this.bufferList.Clear();
			this.readPosition = 0;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000023D9 File Offset: 0x000005D9
		public void WriteByte(byte _input)
		{
			this.bufferList.Add(_input);
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000023E9 File Offset: 0x000005E9
		public void WriteBytes(byte[] _input)
		{
			this.bufferList.AddRange(_input);
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000023FC File Offset: 0x000005FC
		public void WriteInteger(int _input)
		{
			byte[] bytes = BitConverter.GetBytes(_input);
			bool isLittleEndian = BitConverter.IsLittleEndian;
			if (isLittleEndian)
			{
				Array.Reverse<byte>(bytes);
			}
			this.bufferList.AddRange(bytes);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002430 File Offset: 0x00000630
		public void WriteLong(long _input)
		{
			byte[] bytes = BitConverter.GetBytes(_input);
			bool isLittleEndian = BitConverter.IsLittleEndian;
			if (isLittleEndian)
			{
				Array.Reverse<byte>(bytes);
			}
			this.bufferList.AddRange(bytes);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002464 File Offset: 0x00000664
		public void WriteFloat(float _input)
		{
			byte[] bytes = BitConverter.GetBytes(_input);
			bool isLittleEndian = BitConverter.IsLittleEndian;
			if (isLittleEndian)
			{
				Array.Reverse<byte>(bytes);
			}
			this.bufferList.AddRange(bytes);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002498 File Offset: 0x00000698
		public void WriteString(string _input)
		{
			this.WriteInteger((_input != null) ? _input.Length : 0);
			this.bufferList.AddRange(Encoding.ASCII.GetBytes(_input ?? ""));
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000024D0 File Offset: 0x000006D0
		public byte ReadByte(bool _peek = true)
		{
			byte b = this.bufferList[this.readPosition];
			bool flag = _peek && this.bufferList.Count > this.readPosition;
			if (flag)
			{
				this.readPosition++;
			}
			return b;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002524 File Offset: 0x00000724
		public byte[] ReadBytes(int _length, bool _peek = true)
		{
			byte[] array = this.bufferList.GetRange(this.readPosition, _length).ToArray();
			bool flag = _peek && this.bufferList.Count > this.readPosition;
			if (flag)
			{
				this.readPosition += _length;
			}
			return array;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000257C File Offset: 0x0000077C
		public int ReadInteger(bool _peek = true)
		{
			int @int = DataBuffer.GetInt32(this.bufferList, this.readPosition);
			bool flag = _peek && this.bufferList.Count > this.readPosition;
			if (flag)
			{
				this.readPosition += 4;
			}
			return @int;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000025D0 File Offset: 0x000007D0
		public long ReadLong(bool _peek = true)
		{
			long @long = DataBuffer.GetLong(this.bufferList, this.readPosition);
			bool flag = _peek && this.bufferList.Count > this.readPosition;
			if (flag)
			{
				this.readPosition += 8;
			}
			return @long;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002624 File Offset: 0x00000824
		public float ReadFloat(bool _peek = true)
		{
			float @float = DataBuffer.GetFloat(this.bufferList, this.readPosition);
			bool flag = _peek && this.bufferList.Count > this.readPosition;
			if (flag)
			{
				this.readPosition += 4;
			}
			return @float;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002678 File Offset: 0x00000878
		public string ReadString(bool _peek = true)
		{
			int num = this.ReadInteger(true);
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.bufferList[this.readPosition + i];
			}
			string @string = Encoding.ASCII.GetString(array, 0, num);
			bool flag = _peek && this.bufferList.Count > this.readPosition;
			if (flag)
			{
				this.readPosition += num;
			}
			return @string;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002704 File Offset: 0x00000904
		private void Dispose(bool _disposing)
		{
			bool flag = !this.disposedValue;
			if (flag)
			{
				if (_disposing)
				{
					this.bufferList.Clear();
				}
				this.readPosition = 0;
			}
			this.disposedValue = true;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002742 File Offset: 0x00000942
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002754 File Offset: 0x00000954
		public static int GetInt32(List<byte> _buffer, int _startIndex)
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = _buffer[_startIndex + i];
			}
			bool isLittleEndian = BitConverter.IsLittleEndian;
			if (isLittleEndian)
			{
				Array.Reverse<byte>(array);
			}
			return BitConverter.ToInt32(array, 0);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000027A8 File Offset: 0x000009A8
		public static int GetInt32(byte[] _buffer, int _startIndex)
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = _buffer[_startIndex + i];
			}
			bool isLittleEndian = BitConverter.IsLittleEndian;
			if (isLittleEndian)
			{
				Array.Reverse<byte>(array);
			}
			return BitConverter.ToInt32(array, 0);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000027F8 File Offset: 0x000009F8
		public static long GetLong(List<byte> _buffer, int _startIndex)
		{
			byte[] array = new byte[8];
			for (int i = 0; i < 8; i++)
			{
				array[i] = _buffer[_startIndex + i];
			}
			bool isLittleEndian = BitConverter.IsLittleEndian;
			if (isLittleEndian)
			{
				Array.Reverse<byte>(array);
			}
			return BitConverter.ToInt64(array, 0);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x0000284C File Offset: 0x00000A4C
		public static long GetLong(byte[] _buffer, int _startIndex)
		{
			byte[] array = new byte[8];
			for (int i = 0; i < 8; i++)
			{
				array[i] = _buffer[_startIndex + i];
			}
			bool isLittleEndian = BitConverter.IsLittleEndian;
			if (isLittleEndian)
			{
				Array.Reverse<byte>(array);
			}
			return BitConverter.ToInt64(array, 0);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x0000289C File Offset: 0x00000A9C
		public static float GetFloat(List<byte> _buffer, int _startIndex)
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = _buffer[_startIndex + i];
			}
			bool isLittleEndian = BitConverter.IsLittleEndian;
			if (isLittleEndian)
			{
				Array.Reverse<byte>(array);
			}
			return BitConverter.ToSingle(array, 0);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000028F0 File Offset: 0x00000AF0
		public static float GetFloat(byte[] _buffer, int _startIndex)
		{
			byte[] array = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = _buffer[_startIndex + i];
			}
			bool isLittleEndian = BitConverter.IsLittleEndian;
			if (isLittleEndian)
			{
				Array.Reverse<byte>(array);
			}
			return BitConverter.ToSingle(array, 0);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002940 File Offset: 0x00000B40
		public static DataBuffer FromBytes(byte[] _data)
		{
			DataBuffer dataBuffer = new DataBuffer(1000);
			dataBuffer.WriteBytes(_data);
			return dataBuffer;
		}

		// Token: 0x04000008 RID: 8
		public int OriginID;

		// Token: 0x04000009 RID: 9
		public int Sent = 0;

		// Token: 0x0400000A RID: 10
		private List<byte> bufferList;

		// Token: 0x0400000B RID: 11
		private int readPosition;

		// Token: 0x0400000C RID: 12
		private bool disposedValue = false;
	}
}
