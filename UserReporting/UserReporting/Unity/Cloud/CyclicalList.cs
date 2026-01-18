using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Cloud
{
	// Token: 0x02000011 RID: 17
	public class CyclicalList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		// Token: 0x0600003F RID: 63 RVA: 0x0000375F File Offset: 0x0000195F
		public CyclicalList(int capacity)
		{
			this.items = new T[capacity];
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00003773 File Offset: 0x00001973
		public int Capacity
		{
			get
			{
				return this.items.Length;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000041 RID: 65 RVA: 0x0000377D File Offset: 0x0000197D
		public int Count
		{
			get
			{
				return this.count;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000042 RID: 66 RVA: 0x00003785 File Offset: 0x00001985
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000006 RID: 6
		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= this.count)
				{
					throw new IndexOutOfRangeException();
				}
				return this.items[this.GetPointer(index)];
			}
			set
			{
				if (index < 0 || index >= this.count)
				{
					throw new IndexOutOfRangeException();
				}
				this.items[this.GetPointer(index)] = value;
			}
		}

		// Token: 0x06000045 RID: 69 RVA: 0x000037D8 File Offset: 0x000019D8
		public void Add(T item)
		{
			this.items[this.nextPointer] = item;
			this.count++;
			if (this.count > this.items.Length)
			{
				this.count = this.items.Length;
			}
			this.nextPointer++;
			if (this.nextPointer >= this.items.Length)
			{
				this.nextPointer = 0;
			}
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003848 File Offset: 0x00001A48
		public void Clear()
		{
			this.count = 0;
			this.nextPointer = 0;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00003858 File Offset: 0x00001A58
		public bool Contains(T item)
		{
			foreach (T t in this)
			{
				if (t.Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x000038B8 File Offset: 0x00001AB8
		public void CopyTo(T[] array, int arrayIndex)
		{
			int num = 0;
			foreach (T t in this)
			{
				int num2 = arrayIndex + num;
				if (num2 >= array.Length)
				{
					break;
				}
				array[num2] = t;
				num++;
			}
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003914 File Offset: 0x00001B14
		public IEnumerator<T> GetEnumerator()
		{
			return new CyclicalList<T>.Enumerator(this);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00003921 File Offset: 0x00001B21
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00003929 File Offset: 0x00001B29
		public T GetNextEviction()
		{
			return this.items[this.nextPointer];
		}

		// Token: 0x0600004C RID: 76 RVA: 0x0000393C File Offset: 0x00001B3C
		private int GetPointer(int index)
		{
			if (index < 0 || index >= this.count)
			{
				throw new IndexOutOfRangeException();
			}
			if (this.count < this.items.Length)
			{
				return index;
			}
			return (this.nextPointer + index) % this.count;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003974 File Offset: 0x00001B74
		public int IndexOf(T item)
		{
			int num = 0;
			foreach (T t in this)
			{
				if (t.Equals(item))
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x000039D8 File Offset: 0x00001BD8
		public void Insert(int index, T item)
		{
			if (index < 0 || index >= this.count)
			{
				throw new IndexOutOfRangeException();
			}
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000039ED File Offset: 0x00001BED
		public bool Remove(T item)
		{
			return false;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000039F0 File Offset: 0x00001BF0
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= this.count)
			{
				throw new IndexOutOfRangeException();
			}
		}

		// Token: 0x0400002F RID: 47
		private int count;

		// Token: 0x04000030 RID: 48
		private T[] items;

		// Token: 0x04000031 RID: 49
		private int nextPointer;

		// Token: 0x0200003D RID: 61
		private struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			// Token: 0x0600020E RID: 526 RVA: 0x000093E5 File Offset: 0x000075E5
			public Enumerator(CyclicalList<T> list)
			{
				this.list = list;
				this.currentIndex = -1;
			}

			// Token: 0x17000078 RID: 120
			// (get) Token: 0x0600020F RID: 527 RVA: 0x000093F8 File Offset: 0x000075F8
			public T Current
			{
				get
				{
					if (this.currentIndex < 0 || this.currentIndex >= this.list.Count)
					{
						return default(T);
					}
					return this.list[this.currentIndex];
				}
			}

			// Token: 0x17000079 RID: 121
			// (get) Token: 0x06000210 RID: 528 RVA: 0x0000943C File Offset: 0x0000763C
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			// Token: 0x06000211 RID: 529 RVA: 0x00009449 File Offset: 0x00007649
			public void Dispose()
			{
			}

			// Token: 0x06000212 RID: 530 RVA: 0x0000944B File Offset: 0x0000764B
			public bool MoveNext()
			{
				this.currentIndex++;
				return this.currentIndex < this.list.count;
			}

			// Token: 0x06000213 RID: 531 RVA: 0x0000946E File Offset: 0x0000766E
			public void Reset()
			{
				this.currentIndex = 0;
			}

			// Token: 0x040000F8 RID: 248
			private int currentIndex;

			// Token: 0x040000F9 RID: 249
			private CyclicalList<T> list;
		}
	}
}
