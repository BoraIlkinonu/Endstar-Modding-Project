using System;
using System.Linq;

namespace Endless
{
	// Token: 0x02000009 RID: 9
	public static class Utils
	{
		// Token: 0x0600001A RID: 26 RVA: 0x00003E6C File Offset: 0x0000206C
		public static int NextUniqueID(ref int ids, int min, int max)
		{
			long num = (long)ids.Clamp(min, max);
			int num2 = ids + 1;
			ids = num2;
			long num3 = (long)num2;
			bool flag = num3 > (long)max;
			if (flag)
			{
				num3 = (long)min;
			}
			ids = (int)num3;
			return (int)num;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00003EA8 File Offset: 0x000020A8
		public static string GenerateRandomStringId()
		{
			return Guid.NewGuid().ToString();
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00003ED0 File Offset: 0x000020D0
		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
		{
			bool flag = val.CompareTo(min) < 0;
			T t;
			if (flag)
			{
				t = min;
			}
			else
			{
				bool flag2 = val.CompareTo(max) > 0;
				if (flag2)
				{
					t = max;
				}
				else
				{
					t = val;
				}
			}
			return t;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00003F18 File Offset: 0x00002118
		public static string ByteArrayToString(byte[] _byteArray)
		{
			string text = BitConverter.ToString(_byteArray);
			return text.Replace("-", "");
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00003F44 File Offset: 0x00002144
		public static bool ValidateIPv4Address(string _ipAddress)
		{
			bool flag = string.IsNullOrWhiteSpace(_ipAddress);
			bool flag2;
			if (flag)
			{
				flag2 = false;
			}
			else
			{
				string[] array = _ipAddress.Split('.', StringSplitOptions.None);
				bool flag3 = array.Length != 4;
				byte _tempForParsing;
				flag2 = !flag3 && array.All((string r) => byte.TryParse(r, out _tempForParsing));
			}
			return flag2;
		}
	}
}
