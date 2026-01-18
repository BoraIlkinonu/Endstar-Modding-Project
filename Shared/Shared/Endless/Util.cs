using System;
using System.Collections.Generic;
using System.Linq;

namespace Endless
{
	// Token: 0x0200002C RID: 44
	public static class Util
	{
		// Token: 0x06000133 RID: 307 RVA: 0x00008368 File Offset: 0x00006568
		public static int GetRandomInt(int _min, int _max)
		{
			Random random = Util.random;
			int num;
			lock (random)
			{
				num = Util.random.Next(_min, _max);
			}
			return num;
		}

		// Token: 0x06000134 RID: 308 RVA: 0x000083B0 File Offset: 0x000065B0
		public static T PickRandom<T>(IList<T> _list)
		{
			if (_list == null || _list.Count == 0)
			{
				return default(T);
			}
			return _list[Util.GetRandomInt(0, _list.Count)];
		}

		// Token: 0x06000135 RID: 309 RVA: 0x000083E4 File Offset: 0x000065E4
		public static IList<T> PickRandom<T>(IList<T> _list, int _count)
		{
			if (_list.Count <= _count)
			{
				return _list;
			}
			List<T> list = new List<T>();
			while (list.Count < _count)
			{
				T t = Util.PickRandom<T>(_list);
				if (!list.Contains(t))
				{
					list.Add(t);
				}
			}
			return list;
		}

		// Token: 0x06000136 RID: 310 RVA: 0x00008428 File Offset: 0x00006628
		public static T RandomButNot<T>(IList<T> _list, T _butnot)
		{
			int num = 0;
			for (;;)
			{
				T t = Util.PickRandom<T>(_list);
				num++;
				if (num > 1000)
				{
					break;
				}
				if (!t.Equals(_butnot))
				{
					return t;
				}
			}
			throw new ArgumentOutOfRangeException("Safety counter reached!");
		}

		// Token: 0x06000137 RID: 311 RVA: 0x00008470 File Offset: 0x00006670
		public static T RandomButNot<T>(IList<T> _list, IList<T> _butnot)
		{
			int num = 0;
			for (;;)
			{
				T t = Util.PickRandom<T>(_list);
				num++;
				if (num > 1000)
				{
					break;
				}
				if (!_butnot.Contains(t))
				{
					return t;
				}
			}
			throw new ArgumentOutOfRangeException("Safety counter reached!");
		}

		// Token: 0x06000138 RID: 312 RVA: 0x000084A8 File Offset: 0x000066A8
		public static List<T> Shuffle<T>(this List<T> _list)
		{
			List<T> list = new List<T>(_list);
			for (int i = 0; i < list.Count - 2; i++)
			{
				int num = Util.random.Next(i, list.Count);
				T t = list[i];
				list[i] = list[num];
				list[num] = t;
			}
			return list;
		}

		// Token: 0x06000139 RID: 313 RVA: 0x00008500 File Offset: 0x00006700
		public static int NextUniqueID(ref int _ids, int min, int max)
		{
			int num = (int)((long)_ids.Clamp(min, max));
			int num2 = _ids + 1;
			_ids = num2;
			long num3 = (long)num2;
			if (num3 > (long)max)
			{
				num3 = (long)min;
			}
			_ids = (int)num3;
			return num;
		}

		// Token: 0x0600013A RID: 314 RVA: 0x0000852F File Offset: 0x0000672F
		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
		{
			if (val.CompareTo(min) < 0)
			{
				return min;
			}
			if (val.CompareTo(max) > 0)
			{
				return max;
			}
			return val;
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00008558 File Offset: 0x00006758
		public static string ByteArrayToString(byte[] _byteArray)
		{
			return BitConverter.ToString(_byteArray).Replace("-", "");
		}

		// Token: 0x0600013C RID: 316 RVA: 0x00008570 File Offset: 0x00006770
		public static bool ValidateIPv4Address(string _ipAddress)
		{
			if (string.IsNullOrWhiteSpace(_ipAddress))
			{
				return false;
			}
			string[] array = _ipAddress.Split('.', StringSplitOptions.None);
			byte _tempForParsing;
			return array.Length == 4 && array.All((string r) => byte.TryParse(r, out _tempForParsing));
		}

		// Token: 0x0400009E RID: 158
		private static Random random = new Random();
	}
}
