using System;

namespace Endless.Gameplay
{
	// Token: 0x02000363 RID: 867
	public class CircularBuffer<T>
	{
		// Token: 0x0600164D RID: 5709 RVA: 0x000690A2 File Offset: 0x000672A2
		public CircularBuffer(int size)
		{
			this.size = size;
			this.buffer = new T[size];
		}

		// Token: 0x0600164E RID: 5710 RVA: 0x000690C0 File Offset: 0x000672C0
		public void Add(T value)
		{
			int num = this.Count;
			this.Count = num + 1;
			this.buffer[this.headIndex] = value;
			this.headIndex = this.IncrementIndex(this.headIndex);
			if (this.Count > this.size)
			{
				this.tailIndex = this.IncrementIndex(this.tailIndex);
				num = this.Count;
				this.Count = num - 1;
			}
		}

		// Token: 0x0600164F RID: 5711 RVA: 0x00069131 File Offset: 0x00067331
		public void Clear()
		{
			this.tailIndex = 0;
			this.headIndex = 0;
			this.Count = 0;
		}

		// Token: 0x06001650 RID: 5712 RVA: 0x00069148 File Offset: 0x00067348
		public void CopyTo(T[] array, int arrayIndex = 0)
		{
			if (array == null)
			{
				throw new ArgumentNullException(string.Format("The array cannot be null {0}", this));
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException(string.Format("The starting array index cannot be negative {0}", this));
			}
			if (this.Count > array.Length - arrayIndex)
			{
				throw new ArgumentException("The destination array has fewer elements than the collection");
			}
			for (int i = 0; i < this.Count; i++)
			{
				int num = (this.tailIndex + i) % this.size;
				array[i + arrayIndex] = this.buffer[num];
			}
		}

		// Token: 0x170004B9 RID: 1209
		// (get) Token: 0x06001651 RID: 5713 RVA: 0x000691CC File Offset: 0x000673CC
		// (set) Token: 0x06001652 RID: 5714 RVA: 0x000691D4 File Offset: 0x000673D4
		public int Count
		{
			get
			{
				return this.count;
			}
			private set
			{
				this.count = value;
			}
		}

		// Token: 0x06001653 RID: 5715 RVA: 0x000691DD File Offset: 0x000673DD
		public int IncrementIndex(int index)
		{
			index++;
			if (index != this.size)
			{
				return index;
			}
			return 0;
		}

		// Token: 0x170004BA RID: 1210
		public T this[int i]
		{
			get
			{
				int num = (this.tailIndex + i) % this.size;
				return this.buffer[num];
			}
		}

		// Token: 0x0400120C RID: 4620
		private readonly T[] buffer;

		// Token: 0x0400120D RID: 4621
		private readonly int size;

		// Token: 0x0400120E RID: 4622
		private int headIndex;

		// Token: 0x0400120F RID: 4623
		private int tailIndex;

		// Token: 0x04001210 RID: 4624
		private int count;
	}
}
