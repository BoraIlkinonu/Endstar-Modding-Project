using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000291 RID: 657
	public class RingBuffer<T> where T : IFrameInfo
	{
		// Token: 0x170002C1 RID: 705
		// (get) Token: 0x06000E95 RID: 3733 RVA: 0x0004DC9A File Offset: 0x0004BE9A
		protected T[] RingBufferArray
		{
			get
			{
				return this.ringBufferArray;
			}
		}

		// Token: 0x06000E96 RID: 3734 RVA: 0x0004DCA4 File Offset: 0x0004BEA4
		public void UpdateValue(ref T value)
		{
			this.lastUpdated = Mathf.Max(this.lastUpdated, (int)value.NetFrame);
			checked
			{
				this.ringBufferArray[(int)((IntPtr)(unchecked((ulong)value.NetFrame % (ulong)((long)this.count))))] = value;
			}
		}

		// Token: 0x06000E97 RID: 3735 RVA: 0x0004DCF5 File Offset: 0x0004BEF5
		public int GetUseIndexOfFrame(uint frame)
		{
			return (int)(((long)this.lastUpdated > (long)((ulong)frame)) ? ((ulong)frame % (ulong)((long)this.count)) : ((ulong)((long)(this.lastUpdated % this.count))));
		}

		// Token: 0x06000E98 RID: 3736 RVA: 0x0004DD1D File Offset: 0x0004BF1D
		public T GetValue(uint frame)
		{
			if ((long)this.lastUpdated <= (long)((ulong)frame))
			{
				return this.ringBufferArray[this.lastUpdated % this.count];
			}
			checked
			{
				return this.ringBufferArray[(int)((IntPtr)(unchecked((ulong)frame % (ulong)((long)this.count))))];
			}
		}

		// Token: 0x06000E99 RID: 3737 RVA: 0x0004DD59 File Offset: 0x0004BF59
		public ref T GetAtPosition(uint frame)
		{
			checked
			{
				return ref this.ringBufferArray[(int)((IntPtr)(unchecked((ulong)frame % (ulong)((long)this.count))))];
			}
		}

		// Token: 0x06000E9A RID: 3738 RVA: 0x0004DD74 File Offset: 0x0004BF74
		public RingBuffer(int ringCount)
		{
			this.count = Mathf.Max(1, ringCount);
			this.lastUpdated = 0;
			this.ringBufferArray = new T[this.count];
			for (int i = 0; i < this.ringBufferArray.Length; i++)
			{
				this.ringBufferArray[i].Initialize();
			}
		}

		// Token: 0x06000E9B RID: 3739 RVA: 0x0004DD59 File Offset: 0x0004BF59
		public ref T GetReferenceFromBuffer(uint frame)
		{
			checked
			{
				return ref this.ringBufferArray[(int)((IntPtr)(unchecked((ulong)frame % (ulong)((long)this.count))))];
			}
		}

		// Token: 0x06000E9C RID: 3740 RVA: 0x0004DDD8 File Offset: 0x0004BFD8
		public void FrameUpdated(uint frame)
		{
			this.lastUpdated = Mathf.Max(this.lastUpdated, (int)frame);
		}

		// Token: 0x06000E9D RID: 3741 RVA: 0x0004DDEC File Offset: 0x0004BFEC
		public void Clear()
		{
			Array.Clear(this.ringBufferArray, 0, this.count);
		}

		// Token: 0x04000D13 RID: 3347
		private int count;

		// Token: 0x04000D14 RID: 3348
		protected int lastUpdated;

		// Token: 0x04000D15 RID: 3349
		private T[] ringBufferArray;
	}
}
