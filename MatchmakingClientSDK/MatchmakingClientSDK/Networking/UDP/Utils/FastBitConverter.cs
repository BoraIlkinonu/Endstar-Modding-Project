using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Networking.UDP.Utils
{
	// Token: 0x02000038 RID: 56
	public static class FastBitConverter
	{
		// Token: 0x06000186 RID: 390 RVA: 0x00009A5C File Offset: 0x00007C5C
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void WriteLittleEndian(byte[] buffer, int offset, ulong data)
		{
			buffer[offset] = (byte)data;
			buffer[offset + 1] = (byte)(data >> 8);
			buffer[offset + 2] = (byte)(data >> 16);
			buffer[offset + 3] = (byte)(data >> 24);
			buffer[offset + 4] = (byte)(data >> 32);
			buffer[offset + 5] = (byte)(data >> 40);
			buffer[offset + 6] = (byte)(data >> 48);
			buffer[offset + 7] = (byte)(data >> 56);
		}

		// Token: 0x06000187 RID: 391 RVA: 0x00009AB3 File Offset: 0x00007CB3
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void WriteLittleEndian(byte[] buffer, int offset, int data)
		{
			buffer[offset] = (byte)data;
			buffer[offset + 1] = (byte)(data >> 8);
			buffer[offset + 2] = (byte)(data >> 16);
			buffer[offset + 3] = (byte)(data >> 24);
		}

		// Token: 0x06000188 RID: 392 RVA: 0x00009AD7 File Offset: 0x00007CD7
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteLittleEndian(byte[] buffer, int offset, short data)
		{
			buffer[offset] = (byte)data;
			buffer[offset + 1] = (byte)(data >> 8);
		}

		// Token: 0x06000189 RID: 393 RVA: 0x00009AE8 File Offset: 0x00007CE8
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBytes(byte[] bytes, int startIndex, double value)
		{
			FastBitConverter.ConverterHelperDouble converterHelperDouble = new FastBitConverter.ConverterHelperDouble
			{
				Adouble = value
			};
			FastBitConverter.WriteLittleEndian(bytes, startIndex, converterHelperDouble.Along);
		}

		// Token: 0x0600018A RID: 394 RVA: 0x00009B14 File Offset: 0x00007D14
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBytes(byte[] bytes, int startIndex, float value)
		{
			FastBitConverter.ConverterHelperFloat converterHelperFloat = new FastBitConverter.ConverterHelperFloat
			{
				Afloat = value
			};
			FastBitConverter.WriteLittleEndian(bytes, startIndex, converterHelperFloat.Aint);
		}

		// Token: 0x0600018B RID: 395 RVA: 0x00009B40 File Offset: 0x00007D40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBytes(byte[] bytes, int startIndex, short value)
		{
			FastBitConverter.WriteLittleEndian(bytes, startIndex, value);
		}

		// Token: 0x0600018C RID: 396 RVA: 0x00009B4A File Offset: 0x00007D4A
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBytes(byte[] bytes, int startIndex, ushort value)
		{
			FastBitConverter.WriteLittleEndian(bytes, startIndex, (short)value);
		}

		// Token: 0x0600018D RID: 397 RVA: 0x00009B55 File Offset: 0x00007D55
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBytes(byte[] bytes, int startIndex, int value)
		{
			FastBitConverter.WriteLittleEndian(bytes, startIndex, value);
		}

		// Token: 0x0600018E RID: 398 RVA: 0x00009B5F File Offset: 0x00007D5F
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBytes(byte[] bytes, int startIndex, uint value)
		{
			FastBitConverter.WriteLittleEndian(bytes, startIndex, (int)value);
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00009B69 File Offset: 0x00007D69
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBytes(byte[] bytes, int startIndex, long value)
		{
			FastBitConverter.WriteLittleEndian(bytes, startIndex, (ulong)value);
		}

		// Token: 0x06000190 RID: 400 RVA: 0x00009B73 File Offset: 0x00007D73
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetBytes(byte[] bytes, int startIndex, ulong value)
		{
			FastBitConverter.WriteLittleEndian(bytes, startIndex, value);
		}

		// Token: 0x0200008D RID: 141
		[StructLayout(LayoutKind.Explicit)]
		private struct ConverterHelperDouble
		{
			// Token: 0x040002E6 RID: 742
			[FieldOffset(0)]
			public ulong Along;

			// Token: 0x040002E7 RID: 743
			[FieldOffset(0)]
			public double Adouble;
		}

		// Token: 0x0200008E RID: 142
		[StructLayout(LayoutKind.Explicit)]
		private struct ConverterHelperFloat
		{
			// Token: 0x040002E8 RID: 744
			[FieldOffset(0)]
			public int Aint;

			// Token: 0x040002E9 RID: 745
			[FieldOffset(0)]
			public float Afloat;
		}
	}
}
