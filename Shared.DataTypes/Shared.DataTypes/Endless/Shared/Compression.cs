using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000009 RID: 9
	public static class Compression
	{
		// Token: 0x06000025 RID: 37 RVA: 0x000023C1 File Offset: 0x000005C1
		private static int ToBit(this bool value)
		{
			if (!value)
			{
				return 0;
			}
			return 1;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000023C9 File Offset: 0x000005C9
		private static bool CheckBool(this byte thisByte, int bitToCheck)
		{
			return ((int)thisByte & (1 << bitToCheck)) == 1 << bitToCheck;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000023DB File Offset: 0x000005DB
		private static int ByteToInt(byte b)
		{
			return (int)(b - 128);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000023E4 File Offset: 0x000005E4
		private static byte IntToByte(int i)
		{
			if (i + 128 >= 255)
			{
				return byte.MaxValue;
			}
			return (byte)(i + 128);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002404 File Offset: 0x00000604
		private static ushort FloatToUShort(float value, float minValue, float maxValue)
		{
			int num = 65535;
			float num2 = maxValue - minValue;
			return (ushort)((value - minValue) / num2 * (float)num);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002428 File Offset: 0x00000628
		private static float UShortToFloat(ushort value, float minTarget, float maxTarget)
		{
			float num = maxTarget - minTarget;
			ushort maxValue = ushort.MaxValue;
			ushort num2 = value;
			return minTarget + (float)num2 / (float)maxValue * num;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x0000244B File Offset: 0x0000064B
		public static void SerializeIntToByteClamped<T>(BufferSerializer<T> serializer, int value) where T : IReaderWriter
		{
			Compression.byteValue = Compression.IntToByte(value);
			serializer.SerializeValue(ref Compression.byteValue);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002464 File Offset: 0x00000664
		public static int DeserializeIntFromByteClamped<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			return Compression.ByteToInt(Compression.byteValue);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x0000247C File Offset: 0x0000067C
		public static void SerializeUIntToByteClamped<T>(BufferSerializer<T> serializer, uint value) where T : IReaderWriter
		{
			Compression.byteValue = ((value < 255U) ? ((byte)value) : byte.MaxValue);
			serializer.SerializeValue(ref Compression.byteValue);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000024A0 File Offset: 0x000006A0
		public static uint DeserializeUIntFromByteClamped<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			return (uint)Compression.byteValue;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000024B4 File Offset: 0x000006B4
		public static void SerializeFloatToUShort<T>(BufferSerializer<T> serializer, float value, float minValue, float maxValue) where T : IReaderWriter
		{
			Compression.ushortValue = Compression.FloatToUShort(value, minValue, maxValue);
			serializer.SerializeValue<ushort>(ref Compression.ushortValue, default(FastBufferWriter.ForPrimitives));
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000024E4 File Offset: 0x000006E4
		public static float DeserializeFloatFromUShort<T>(BufferSerializer<T> serializer, float minValue, float maxValue) where T : IReaderWriter
		{
			serializer.SerializeValue<ushort>(ref Compression.ushortValue, default(FastBufferWriter.ForPrimitives));
			return Compression.UShortToFloat(Compression.ushortValue, minValue, maxValue);
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002512 File Offset: 0x00000712
		public static void SerializeFloatDecimal1<T>(BufferSerializer<T> serializer, float floatValue) where T : IReaderWriter
		{
			Compression.SerializeLong<T>(serializer, (long)(floatValue * 10f));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002522 File Offset: 0x00000722
		public static float DeserializeFloatDecimal1<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			return (float)Compression.DeserializeLong<T>(serializer) / 10f;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002531 File Offset: 0x00000731
		public static void SerializeFloatDecimal2<T>(BufferSerializer<T> serializer, float floatValue) where T : IReaderWriter
		{
			Compression.SerializeLong<T>(serializer, (long)(floatValue * 100f));
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002541 File Offset: 0x00000741
		public static float DeserializeFloatDecimal2<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			return (float)Compression.DeserializeLong<T>(serializer) / 100f;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002550 File Offset: 0x00000750
		public static void SerializeColor<T>(BufferSerializer<T> serializer, Color color) where T : IReaderWriter
		{
			Compression.SerializeFloatToUShort<T>(serializer, color.r, 0f, 1f);
			Compression.SerializeFloatToUShort<T>(serializer, color.g, 0f, 1f);
			Compression.SerializeFloatToUShort<T>(serializer, color.b, 0f, 1f);
			Compression.SerializeFloatToUShort<T>(serializer, color.a, 0f, 1f);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x000025B8 File Offset: 0x000007B8
		public static Color DeserializeColor<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			Compression.colorValue.r = Compression.DeserializeFloatFromUShort<T>(serializer, 0f, 1f);
			Compression.colorValue.g = Compression.DeserializeFloatFromUShort<T>(serializer, 0f, 1f);
			Compression.colorValue.b = Compression.DeserializeFloatFromUShort<T>(serializer, 0f, 1f);
			Compression.colorValue.a = Compression.DeserializeFloatFromUShort<T>(serializer, 0f, 1f);
			return Compression.colorValue;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002634 File Offset: 0x00000834
		public static void SerializeBoolsToByte<T>(BufferSerializer<T> serializer, bool b0, bool b1, bool b2 = false, bool b3 = false, bool b4 = false, bool b5 = false, bool b6 = false, bool b7 = false) where T : IReaderWriter
		{
			Compression.byteValue = (byte)(b0.ToBit() | (b1.ToBit() << 1) | (b2.ToBit() << 2) | (b3.ToBit() << 3) | (b4.ToBit() << 4) | (b5.ToBit() << 5) | (b6.ToBit() << 6) | (b7.ToBit() << 7));
			serializer.SerializeValue(ref Compression.byteValue);
		}

		// Token: 0x06000038 RID: 56 RVA: 0x0000269D File Offset: 0x0000089D
		public static void DeserializeBoolsFromByte<T>(BufferSerializer<T> serializer, ref bool b0, ref bool b1) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			b0 = Compression.byteValue.CheckBool(0);
			b1 = Compression.byteValue.CheckBool(1);
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000026C5 File Offset: 0x000008C5
		public static void DeserializeBoolsFromByte<T>(BufferSerializer<T> serializer, ref bool b0, ref bool b1, ref bool b2) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			b0 = Compression.byteValue.CheckBool(0);
			b1 = Compression.byteValue.CheckBool(1);
			b2 = Compression.byteValue.CheckBool(2);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x000026FC File Offset: 0x000008FC
		public static void DeserializeBoolsFromByte<T>(BufferSerializer<T> serializer, ref bool b0, ref bool b1, ref bool b2, ref bool b3) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			b0 = Compression.byteValue.CheckBool(0);
			b1 = Compression.byteValue.CheckBool(1);
			b2 = Compression.byteValue.CheckBool(2);
			b3 = Compression.byteValue.CheckBool(3);
		}

		// Token: 0x0600003B RID: 59 RVA: 0x0000274C File Offset: 0x0000094C
		public static void DeserializeBoolsFromByte<T>(BufferSerializer<T> serializer, ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			b0 = Compression.byteValue.CheckBool(0);
			b1 = Compression.byteValue.CheckBool(1);
			b2 = Compression.byteValue.CheckBool(2);
			b3 = Compression.byteValue.CheckBool(3);
			b4 = Compression.byteValue.CheckBool(4);
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000027A8 File Offset: 0x000009A8
		public static void DeserializeBoolsFromByte<T>(BufferSerializer<T> serializer, ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			b0 = Compression.byteValue.CheckBool(0);
			b1 = Compression.byteValue.CheckBool(1);
			b2 = Compression.byteValue.CheckBool(2);
			b3 = Compression.byteValue.CheckBool(3);
			b4 = Compression.byteValue.CheckBool(4);
			b5 = Compression.byteValue.CheckBool(5);
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002814 File Offset: 0x00000A14
		public static void DeserializeBoolsFromByte<T>(BufferSerializer<T> serializer, ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			b0 = Compression.byteValue.CheckBool(0);
			b1 = Compression.byteValue.CheckBool(1);
			b2 = Compression.byteValue.CheckBool(2);
			b3 = Compression.byteValue.CheckBool(3);
			b4 = Compression.byteValue.CheckBool(4);
			b5 = Compression.byteValue.CheckBool(5);
			b6 = Compression.byteValue.CheckBool(6);
		}

		// Token: 0x0600003E RID: 62 RVA: 0x0000288C File Offset: 0x00000A8C
		public static void DeserializeBoolsFromByte<T>(BufferSerializer<T> serializer, ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6, ref bool b7) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			b0 = Compression.byteValue.CheckBool(0);
			b1 = Compression.byteValue.CheckBool(1);
			b2 = Compression.byteValue.CheckBool(2);
			b3 = Compression.byteValue.CheckBool(3);
			b4 = Compression.byteValue.CheckBool(4);
			b5 = Compression.byteValue.CheckBool(5);
			b6 = Compression.byteValue.CheckBool(6);
			b7 = Compression.byteValue.CheckBool(7);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002912 File Offset: 0x00000B12
		public static void SerializeUShort<T>(BufferSerializer<T> serializer, ushort value) where T : IReaderWriter
		{
			Compression.SerializeULong<T>(serializer, (ulong)value);
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00002912 File Offset: 0x00000B12
		public static void SerializeUInt<T>(BufferSerializer<T> serializer, uint value) where T : IReaderWriter
		{
			Compression.SerializeULong<T>(serializer, (ulong)value);
		}

		// Token: 0x06000041 RID: 65 RVA: 0x0000291C File Offset: 0x00000B1C
		public static void SerializeULong<T>(BufferSerializer<T> serializer, ulong value) where T : IReaderWriter
		{
			if (value <= 240UL)
			{
				Compression.byteValue = (byte)value;
				serializer.SerializeValue(ref Compression.byteValue);
				return;
			}
			if (value <= 2287UL)
			{
				Compression.byteValue = (byte)((value - 240UL >> 8) + 241UL);
				Compression.byteValue2 = (byte)((value - 240UL) & 255UL);
				serializer.SerializeValue(ref Compression.byteValue);
				serializer.SerializeValue(ref Compression.byteValue2);
				return;
			}
			if (value <= 67823UL)
			{
				Compression.byteValue = 249;
				Compression.byteValue2 = (byte)(value - 2288UL >> 8);
				Compression.byteValue3 = (byte)((value - 2288UL) & 255UL);
				serializer.SerializeValue(ref Compression.byteValue);
				serializer.SerializeValue(ref Compression.byteValue2);
				serializer.SerializeValue(ref Compression.byteValue3);
				return;
			}
			if (value <= 16777215UL)
			{
				Compression.byteValue = 250;
				Compression.byteValue2 = (byte)(value & 255UL);
				Compression.byteValue3 = (byte)((value >> 8) & 255UL);
				Compression.byteValue4 = (byte)((value >> 16) & 255UL);
				serializer.SerializeValue(ref Compression.byteValue);
				serializer.SerializeValue(ref Compression.byteValue2);
				serializer.SerializeValue(ref Compression.byteValue3);
				serializer.SerializeValue(ref Compression.byteValue4);
				return;
			}
			if (value <= (ulong)(-1))
			{
				Compression.byteValue = 251;
				Compression.byteValue2 = (byte)(value & 255UL);
				Compression.byteValue3 = (byte)((value >> 8) & 255UL);
				Compression.byteValue4 = (byte)((value >> 16) & 255UL);
				Compression.byteValue5 = (byte)((value >> 24) & 255UL);
				serializer.SerializeValue(ref Compression.byteValue);
				serializer.SerializeValue(ref Compression.byteValue2);
				serializer.SerializeValue(ref Compression.byteValue3);
				serializer.SerializeValue(ref Compression.byteValue4);
				serializer.SerializeValue(ref Compression.byteValue5);
				return;
			}
			if (value <= 1099511627775UL)
			{
				Compression.byteValue = 252;
				Compression.byteValue2 = (byte)(value & 255UL);
				Compression.byteValue3 = (byte)((value >> 8) & 255UL);
				Compression.byteValue4 = (byte)((value >> 16) & 255UL);
				Compression.byteValue5 = (byte)((value >> 24) & 255UL);
				Compression.byteValue6 = (byte)((value >> 32) & 255UL);
				serializer.SerializeValue(ref Compression.byteValue);
				serializer.SerializeValue(ref Compression.byteValue2);
				serializer.SerializeValue(ref Compression.byteValue3);
				serializer.SerializeValue(ref Compression.byteValue4);
				serializer.SerializeValue(ref Compression.byteValue5);
				serializer.SerializeValue(ref Compression.byteValue6);
				return;
			}
			if (value <= 281474976710655UL)
			{
				Compression.byteValue = 253;
				Compression.byteValue2 = (byte)(value & 255UL);
				Compression.byteValue3 = (byte)((value >> 8) & 255UL);
				Compression.byteValue4 = (byte)((value >> 16) & 255UL);
				Compression.byteValue5 = (byte)((value >> 24) & 255UL);
				Compression.byteValue6 = (byte)((value >> 32) & 255UL);
				Compression.byteValue7 = (byte)((value >> 40) & 255UL);
				serializer.SerializeValue(ref Compression.byteValue);
				serializer.SerializeValue(ref Compression.byteValue2);
				serializer.SerializeValue(ref Compression.byteValue3);
				serializer.SerializeValue(ref Compression.byteValue4);
				serializer.SerializeValue(ref Compression.byteValue5);
				serializer.SerializeValue(ref Compression.byteValue6);
				serializer.SerializeValue(ref Compression.byteValue7);
				return;
			}
			if (value <= 72057594037927935UL)
			{
				Compression.byteValue = 254;
				Compression.byteValue2 = (byte)(value & 255UL);
				Compression.byteValue3 = (byte)((value >> 8) & 255UL);
				Compression.byteValue4 = (byte)((value >> 16) & 255UL);
				Compression.byteValue5 = (byte)((value >> 24) & 255UL);
				Compression.byteValue6 = (byte)((value >> 32) & 255UL);
				Compression.byteValue7 = (byte)((value >> 40) & 255UL);
				Compression.byteValue8 = (byte)((value >> 48) & 255UL);
				serializer.SerializeValue(ref Compression.byteValue);
				serializer.SerializeValue(ref Compression.byteValue2);
				serializer.SerializeValue(ref Compression.byteValue3);
				serializer.SerializeValue(ref Compression.byteValue4);
				serializer.SerializeValue(ref Compression.byteValue5);
				serializer.SerializeValue(ref Compression.byteValue6);
				serializer.SerializeValue(ref Compression.byteValue7);
				serializer.SerializeValue(ref Compression.byteValue8);
				return;
			}
			Compression.byteValue = byte.MaxValue;
			Compression.byteValue2 = (byte)(value & 255UL);
			Compression.byteValue3 = (byte)((value >> 8) & 255UL);
			Compression.byteValue4 = (byte)((value >> 16) & 255UL);
			Compression.byteValue5 = (byte)((value >> 24) & 255UL);
			Compression.byteValue6 = (byte)((value >> 32) & 255UL);
			Compression.byteValue7 = (byte)((value >> 40) & 255UL);
			Compression.byteValue8 = (byte)((value >> 48) & 255UL);
			Compression.byteValue9 = (byte)((value >> 56) & 255UL);
			serializer.SerializeValue(ref Compression.byteValue);
			serializer.SerializeValue(ref Compression.byteValue2);
			serializer.SerializeValue(ref Compression.byteValue3);
			serializer.SerializeValue(ref Compression.byteValue4);
			serializer.SerializeValue(ref Compression.byteValue5);
			serializer.SerializeValue(ref Compression.byteValue6);
			serializer.SerializeValue(ref Compression.byteValue7);
			serializer.SerializeValue(ref Compression.byteValue8);
			serializer.SerializeValue(ref Compression.byteValue9);
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00002E5D File Offset: 0x0000105D
		public static ushort DeserializeUShort<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			return (ushort)Compression.DeserializeULong<T>(serializer);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002E66 File Offset: 0x00001066
		public static uint DeserializeUInt<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			return (uint)Compression.DeserializeULong<T>(serializer);
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00002E70 File Offset: 0x00001070
		public static ulong DeserializeULong<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Compression.byteValue);
			if (Compression.byteValue < 241)
			{
				return (ulong)Compression.byteValue;
			}
			serializer.SerializeValue(ref Compression.byteValue2);
			if (Compression.byteValue <= 248)
			{
				return 240UL + ((ulong)Compression.byteValue - 241UL << 8) + (ulong)Compression.byteValue2;
			}
			serializer.SerializeValue(ref Compression.byteValue3);
			if (Compression.byteValue == 249)
			{
				return 2288UL + ((ulong)Compression.byteValue2 << 8) + (ulong)Compression.byteValue3;
			}
			serializer.SerializeValue(ref Compression.byteValue4);
			if (Compression.byteValue == 250)
			{
				return (ulong)Compression.byteValue2 + ((ulong)Compression.byteValue3 << 8) + ((ulong)Compression.byteValue4 << 16);
			}
			serializer.SerializeValue(ref Compression.byteValue5);
			if (Compression.byteValue == 251)
			{
				return (ulong)Compression.byteValue2 + ((ulong)Compression.byteValue3 << 8) + ((ulong)Compression.byteValue4 << 16) + ((ulong)Compression.byteValue5 << 24);
			}
			serializer.SerializeValue(ref Compression.byteValue6);
			if (Compression.byteValue == 252)
			{
				return (ulong)Compression.byteValue2 + ((ulong)Compression.byteValue3 << 8) + ((ulong)Compression.byteValue4 << 16) + ((ulong)Compression.byteValue5 << 24) + ((ulong)Compression.byteValue6 << 32);
			}
			serializer.SerializeValue(ref Compression.byteValue7);
			if (Compression.byteValue == 253)
			{
				return (ulong)Compression.byteValue2 + ((ulong)Compression.byteValue3 << 8) + ((ulong)Compression.byteValue4 << 16) + ((ulong)Compression.byteValue5 << 24) + ((ulong)Compression.byteValue6 << 32) + ((ulong)Compression.byteValue7 << 40);
			}
			serializer.SerializeValue(ref Compression.byteValue8);
			if (Compression.byteValue == 254)
			{
				return (ulong)Compression.byteValue2 + ((ulong)Compression.byteValue3 << 8) + ((ulong)Compression.byteValue4 << 16) + ((ulong)Compression.byteValue5 << 24) + ((ulong)Compression.byteValue6 << 32) + ((ulong)Compression.byteValue7 << 40) + ((ulong)Compression.byteValue8 << 48);
			}
			serializer.SerializeValue(ref Compression.byteValue9);
			if (Compression.byteValue == 255)
			{
				return (ulong)Compression.byteValue2 + ((ulong)Compression.byteValue3 << 8) + ((ulong)Compression.byteValue4 << 16) + ((ulong)Compression.byteValue5 << 24) + ((ulong)Compression.byteValue6 << 32) + ((ulong)Compression.byteValue7 << 40) + ((ulong)Compression.byteValue8 << 48) + ((ulong)Compression.byteValue9 << 56);
			}
			throw new IndexOutOfRangeException(string.Format("DecompressVarInt failure: {0}", Compression.byteValue));
		}

		// Token: 0x06000045 RID: 69 RVA: 0x000030DC File Offset: 0x000012DC
		public static void SerializeShort<T>(BufferSerializer<T> serializer, short value) where T : IReaderWriter
		{
			Compression.SerializeLong<T>(serializer, (long)value);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000030DC File Offset: 0x000012DC
		public static void SerializeInt<T>(BufferSerializer<T> serializer, int value) where T : IReaderWriter
		{
			Compression.SerializeLong<T>(serializer, (long)value);
		}

		// Token: 0x06000047 RID: 71 RVA: 0x000030E8 File Offset: 0x000012E8
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SerializeLong<T>(BufferSerializer<T> serializer, long i) where T : IReaderWriter
		{
			ulong num = (ulong)((i >> 63) ^ (i << 1));
			Compression.SerializeULong<T>(serializer, num);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003105 File Offset: 0x00001305
		public static short DeserializeShort<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			return (short)Compression.DeserializeLong<T>(serializer);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x0000310E File Offset: 0x0000130E
		public static int DeserializeInt<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			return (int)Compression.DeserializeLong<T>(serializer);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00003118 File Offset: 0x00001318
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long DeserializeLong<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			ulong num = Compression.DeserializeULong<T>(serializer);
			return (long)((num >> 1) ^ -(long)(num & 1UL));
		}

		// Token: 0x0400000F RID: 15
		private static byte byteValue;

		// Token: 0x04000010 RID: 16
		private static ushort ushortValue;

		// Token: 0x04000011 RID: 17
		private static Color colorValue;

		// Token: 0x04000012 RID: 18
		private static byte byteValue2;

		// Token: 0x04000013 RID: 19
		private static byte byteValue3;

		// Token: 0x04000014 RID: 20
		private static byte byteValue4;

		// Token: 0x04000015 RID: 21
		private static byte byteValue5;

		// Token: 0x04000016 RID: 22
		private static byte byteValue6;

		// Token: 0x04000017 RID: 23
		private static byte byteValue7;

		// Token: 0x04000018 RID: 24
		private static byte byteValue8;

		// Token: 0x04000019 RID: 25
		private static byte byteValue9;
	}
}
