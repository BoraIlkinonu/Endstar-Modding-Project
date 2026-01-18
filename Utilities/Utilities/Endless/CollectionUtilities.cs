using System;
using System.Collections.Generic;

namespace Endless
{
	// Token: 0x02000005 RID: 5
	public static class CollectionUtilities
	{
		// Token: 0x0600000D RID: 13 RVA: 0x000034B8 File Offset: 0x000016B8
		public static T PickRandom<T>(IList<T> _list)
		{
			bool flag = _list == null || _list.Count == 0;
			T t;
			if (flag)
			{
				t = default(T);
			}
			else
			{
				t = _list[CollectionUtilities.random.Next(0, _list.Count)];
			}
			return t;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00003504 File Offset: 0x00001704
		public static IList<T> PickRandom<T>(IList<T> _list, int _count)
		{
			bool flag = _list.Count <= _count;
			IList<T> list;
			if (flag)
			{
				list = _list;
			}
			else
			{
				List<T> list2 = new List<T>();
				while (list2.Count < _count)
				{
					T t = CollectionUtilities.PickRandom<T>(_list);
					bool flag2 = !list2.Contains(t);
					if (flag2)
					{
						list2.Add(t);
					}
				}
				list = list2;
			}
			return list;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00003568 File Offset: 0x00001768
		public static T RandomButNot<T>(IList<T> _list, T _butnot)
		{
			int num = 0;
			for (;;)
			{
				T t = CollectionUtilities.PickRandom<T>(_list);
				num++;
				bool flag = num > 1000;
				if (flag)
				{
					break;
				}
				if (!t.Equals(_butnot))
				{
					return t;
				}
			}
			ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("Safety counter reached!");
			throw ex;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000035C4 File Offset: 0x000017C4
		public static T RandomButNot<T>(IList<T> _list, IList<T> _butnot)
		{
			int num = 0;
			for (;;)
			{
				T t = CollectionUtilities.PickRandom<T>(_list);
				num++;
				bool flag = num > 1000;
				if (flag)
				{
					break;
				}
				if (!_butnot.Contains(t))
				{
					return t;
				}
			}
			ArgumentOutOfRangeException ex = new ArgumentOutOfRangeException("Safety counter reached!");
			throw ex;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00003610 File Offset: 0x00001810
		public static List<T> Shuffle<T>(this List<T> _list)
		{
			List<T> list = new List<T>(_list);
			for (int i = 0; i < list.Count - 2; i++)
			{
				int num = CollectionUtilities.random.Next(i, list.Count);
				T t = list[i];
				list[i] = list[num];
				list[num] = t;
			}
			return list;
		}

		// Token: 0x04000005 RID: 5
		private static Random random = new Random();
	}
}
