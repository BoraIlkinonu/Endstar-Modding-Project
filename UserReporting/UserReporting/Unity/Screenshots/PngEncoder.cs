using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Unity.Screenshots
{
	// Token: 0x0200000D RID: 13
	public static class PngEncoder
	{
		// Token: 0x06000028 RID: 40 RVA: 0x00002F00 File Offset: 0x00001100
		private static uint Adler32(byte[] bytes)
		{
			uint num = 1U;
			uint num2 = 0U;
			foreach (byte b in bytes)
			{
				num = (num + (uint)b) % 65521U;
				num2 = (num2 + num) % 65521U;
			}
			return (num2 << 16) | num;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002F41 File Offset: 0x00001141
		private static void AppendByte(this byte[] data, ref int position, byte value)
		{
			data[position] = value;
			position++;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002F50 File Offset: 0x00001150
		private static void AppendBytes(this byte[] data, ref int position, byte[] value)
		{
			foreach (byte b in value)
			{
				data.AppendByte(ref position, b);
			}
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002F7C File Offset: 0x0000117C
		private static void AppendChunk(this byte[] data, ref int position, string chunkType, byte[] chunkData)
		{
			byte[] chunkTypeBytes = PngEncoder.GetChunkTypeBytes(chunkType);
			if (chunkTypeBytes != null)
			{
				data.AppendInt(ref position, chunkData.Length);
				data.AppendBytes(ref position, chunkTypeBytes);
				data.AppendBytes(ref position, chunkData);
				data.AppendUInt(ref position, PngEncoder.GetChunkCrc(chunkTypeBytes, chunkData));
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002FBC File Offset: 0x000011BC
		private static void AppendInt(this byte[] data, ref int position, int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse<byte>(bytes);
			}
			data.AppendBytes(ref position, bytes);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002FE8 File Offset: 0x000011E8
		private static void AppendUInt(this byte[] data, ref int position, uint value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse<byte>(bytes);
			}
			data.AppendBytes(ref position, bytes);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003014 File Offset: 0x00001214
		private static byte[] Compress(byte[] bytes)
		{
			byte[] array;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
				{
					using (MemoryStream memoryStream2 = new MemoryStream(bytes))
					{
						memoryStream2.WriteTo(deflateStream);
					}
				}
				array = memoryStream.ToArray();
			}
			return array;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003090 File Offset: 0x00001290
		public static byte[] Encode(byte[] dataRgba, int stride)
		{
			if (dataRgba == null)
			{
				throw new ArgumentNullException("dataRgba");
			}
			if (dataRgba.Length == 0)
			{
				throw new ArgumentException("The data length must be greater than 0.");
			}
			if (stride == 0)
			{
				throw new ArgumentException("The stride must be greater than 0.");
			}
			if (stride % 4 != 0)
			{
				throw new ArgumentException("The stride must be evenly divisible by 4.");
			}
			if (dataRgba.Length % 4 != 0)
			{
				throw new ArgumentException("The data must be evenly divisible by 4.");
			}
			if (dataRgba.Length % stride != 0)
			{
				throw new ArgumentException("The data must be evenly divisible by the stride.");
			}
			int num = dataRgba.Length / 4;
			int num2 = stride / 4;
			int num3 = num / num2;
			byte[] array = new byte[13];
			int num4 = 0;
			array.AppendInt(ref num4, num2);
			array.AppendInt(ref num4, num3);
			array.AppendByte(ref num4, 8);
			array.AppendByte(ref num4, 6);
			array.AppendByte(ref num4, 0);
			array.AppendByte(ref num4, 0);
			array.AppendByte(ref num4, 0);
			byte[] array2 = new byte[dataRgba.Length + num3];
			int num5 = 0;
			int num6 = 0;
			for (int i = 0; i < dataRgba.Length; i++)
			{
				if (num6 >= stride)
				{
					num6 = 0;
				}
				if (num6 == 0)
				{
					array2.AppendByte(ref num5, 0);
				}
				array2.AppendByte(ref num5, dataRgba[i]);
				num6++;
			}
			byte[] array3 = PngEncoder.Compress(array2);
			byte[] array4 = new byte[2 + array3.Length + 4];
			int num7 = 0;
			array4.AppendByte(ref num7, 120);
			array4.AppendByte(ref num7, 156);
			array4.AppendBytes(ref num7, array3);
			array4.AppendUInt(ref num7, PngEncoder.Adler32(array2));
			byte[] array5 = new byte[8 + array.Length + 12 + array4.Length + 12 + 12];
			int num8 = 0;
			array5.AppendByte(ref num8, 137);
			array5.AppendByte(ref num8, 80);
			array5.AppendByte(ref num8, 78);
			array5.AppendByte(ref num8, 71);
			array5.AppendByte(ref num8, 13);
			array5.AppendByte(ref num8, 10);
			array5.AppendByte(ref num8, 26);
			array5.AppendByte(ref num8, 10);
			array5.AppendChunk(ref num8, "IHDR", array);
			array5.AppendChunk(ref num8, "IDAT", array4);
			array5.AppendChunk(ref num8, "IEND", new byte[0]);
			return array5;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x0000328B File Offset: 0x0000148B
		public static void EncodeAsync(byte[] dataRgba, int stride, Action<Exception, byte[]> callback)
		{
			ThreadPool.QueueUserWorkItem(delegate(object state)
			{
				try
				{
					byte[] array = PngEncoder.Encode(dataRgba, stride);
					callback(null, array);
				}
				catch (Exception ex)
				{
					callback(ex, null);
					throw;
				}
			}, null);
		}

		// Token: 0x06000031 RID: 49 RVA: 0x000032BC File Offset: 0x000014BC
		private static uint GetChunkCrc(byte[] chunkTypeBytes, byte[] chunkData)
		{
			byte[] array = new byte[chunkTypeBytes.Length + chunkData.Length];
			Array.Copy(chunkTypeBytes, 0, array, 0, chunkTypeBytes.Length);
			Array.Copy(chunkData, 0, array, 4, chunkData.Length);
			return PngEncoder.crc32.Calculate<byte>(array);
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000032FC File Offset: 0x000014FC
		private static byte[] GetChunkTypeBytes(string value)
		{
			char[] array = value.ToCharArray();
			if (array.Length < 4)
			{
				return null;
			}
			byte[] array2 = new byte[4];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = (byte)array[i];
			}
			return array2;
		}

		// Token: 0x04000026 RID: 38
		private static PngEncoder.Crc32 crc32 = new PngEncoder.Crc32();

		// Token: 0x02000039 RID: 57
		public class Crc32
		{
			// Token: 0x060001DA RID: 474 RVA: 0x00008FE4 File Offset: 0x000071E4
			public Crc32()
			{
				this.checksumTable = Enumerable.Range(0, 256).Select(delegate(int i)
				{
					uint num = (uint)i;
					for (int j = 0; j < 8; j++)
					{
						num = (((num & 1U) != 0U) ? (PngEncoder.Crc32.generator ^ (num >> 1)) : (num >> 1));
					}
					return num;
				}).ToArray<uint>();
			}

			// Token: 0x060001DB RID: 475 RVA: 0x00009031 File Offset: 0x00007231
			public uint Calculate<T>(IEnumerable<T> byteStream)
			{
				return ~byteStream.Aggregate(uint.MaxValue, (uint checksumRegister, T currentByte) => this.checksumTable[(int)((checksumRegister & 255U) ^ (uint)Convert.ToByte(currentByte))] ^ (checksumRegister >> 8));
			}

			// Token: 0x040000DD RID: 221
			private static uint generator = 3988292384U;

			// Token: 0x040000DE RID: 222
			private readonly uint[] checksumTable;
		}
	}
}
