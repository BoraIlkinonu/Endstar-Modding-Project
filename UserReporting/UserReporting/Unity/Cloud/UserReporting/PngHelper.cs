using System;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x02000017 RID: 23
	public static class PngHelper
	{
		// Token: 0x06000073 RID: 115 RVA: 0x00003D63 File Offset: 0x00001F63
		public static int GetPngHeightFromBase64Data(string data)
		{
			if (data == null || data.Length < 32)
			{
				return 0;
			}
			byte[] array = PngHelper.Slice(Convert.FromBase64String(data.Substring(0, 32)), 20, 24);
			Array.Reverse<byte>(array);
			return BitConverter.ToInt32(array, 0);
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00003D97 File Offset: 0x00001F97
		public static int GetPngWidthFromBase64Data(string data)
		{
			if (data == null || data.Length < 32)
			{
				return 0;
			}
			byte[] array = PngHelper.Slice(Convert.FromBase64String(data.Substring(0, 32)), 16, 20);
			Array.Reverse<byte>(array);
			return BitConverter.ToInt32(array, 0);
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00003DCC File Offset: 0x00001FCC
		private static byte[] Slice(byte[] source, int start, int end)
		{
			if (end < 0)
			{
				end = source.Length + end;
			}
			int num = end - start;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = source[i + start];
			}
			return array;
		}
	}
}
