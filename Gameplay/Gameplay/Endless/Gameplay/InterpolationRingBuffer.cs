using System;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000E0 RID: 224
	public class InterpolationRingBuffer<T> : RingBuffer<T> where T : IFrameInfo
	{
		// Token: 0x170000CA RID: 202
		// (get) Token: 0x060004DC RID: 1244 RVA: 0x00018DE5 File Offset: 0x00016FE5
		// (set) Token: 0x060004DD RID: 1245 RVA: 0x00018DF0 File Offset: 0x00016FF0
		public double ActiveInterpolationTime
		{
			get
			{
				return this._activeInterpolationTime;
			}
			set
			{
				uint num = 0U;
				while ((ulong)num < (ulong)((long)this.searchCount) && value > this.NextStateStartTime)
				{
					this.ShiftState();
					num += 1U;
				}
				this._activeInterpolationTime = value;
				this.ActiveStateLerpTime = (float)((this._activeInterpolationTime - this.PastStateStartTime) / (double)((float)(this.NextStateStartTime - this.PastStateStartTime)));
			}
		}

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x060004DE RID: 1246 RVA: 0x00018E4A File Offset: 0x0001704A
		// (set) Token: 0x060004DF RID: 1247 RVA: 0x00018E52 File Offset: 0x00017052
		public float ActiveStateLerpTime { get; protected set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x060004E0 RID: 1248 RVA: 0x00018E5B File Offset: 0x0001705B
		public T PastInterpolationState
		{
			get
			{
				return base.RingBufferArray[this.pastInterpolationIndex];
			}
		}

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x060004E1 RID: 1249 RVA: 0x00018E6E File Offset: 0x0001706E
		public double PastStateStartTime
		{
			get
			{
				return NetClock.GetFrameTime(base.RingBufferArray[this.pastInterpolationIndex].NetFrame);
			}
		}

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x060004E2 RID: 1250 RVA: 0x00018E93 File Offset: 0x00017093
		public T NextInterpolationState
		{
			get
			{
				return base.RingBufferArray[this.nextInterpolationIndex];
			}
		}

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x060004E3 RID: 1251 RVA: 0x00018EA6 File Offset: 0x000170A6
		public double NextStateStartTime
		{
			get
			{
				return NetClock.GetFrameTime(base.RingBufferArray[this.nextInterpolationIndex].NetFrame);
			}
		}

		// Token: 0x060004E4 RID: 1252 RVA: 0x00018ECC File Offset: 0x000170CC
		public new void UpdateValue(ref T value)
		{
			base.UpdateValue(ref value);
			uint netFrame = value.NetFrame;
			T t = this.PastInterpolationState;
			if (netFrame > t.NetFrame)
			{
				uint netFrame2 = value.NetFrame;
				t = this.NextInterpolationState;
				if (netFrame2 <= t.NetFrame)
				{
					double frameTime = NetClock.GetFrameTime(value.NetFrame);
					if (this.ActiveInterpolationTime <= frameTime)
					{
						this.SetNext(base.GetUseIndexOfFrame(value.NetFrame));
						this.OnStatesShifted.Invoke(this.PastInterpolationState, this.NextInterpolationState);
						return;
					}
					this.OnStatesShifted.Invoke(this.PastInterpolationState, value);
					this.SetPrevious(base.GetUseIndexOfFrame(value.NetFrame));
				}
			}
		}

		// Token: 0x060004E5 RID: 1253 RVA: 0x00018FA4 File Offset: 0x000171A4
		private void ShiftState()
		{
			this.SetPrevious(this.nextInterpolationIndex);
			T nextInterpolationState = this.NextInterpolationState;
			this.SearchForAndSetNext(nextInterpolationState.NetFrame + 1U);
			this.OnStatesShifted.Invoke(this.PastInterpolationState, this.NextInterpolationState);
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x00018FF0 File Offset: 0x000171F0
		private void SetPrevious(int index)
		{
			this.pastInterpolationIndex = index;
		}

		// Token: 0x060004E7 RID: 1255 RVA: 0x00018FF9 File Offset: 0x000171F9
		private void SetNext(int index)
		{
			this.nextInterpolationIndex = index;
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x00019004 File Offset: 0x00017204
		private void SearchForAndSetNext(uint checkFrame)
		{
			if ((long)this.lastUpdated <= (long)((ulong)checkFrame))
			{
				this.SetNext(this.pastInterpolationIndex);
			}
			uint num = checkFrame;
			while ((ulong)num < (ulong)checkFrame + (ulong)((long)this.searchCount))
			{
				int useIndexOfFrame = base.GetUseIndexOfFrame(num);
				if (base.RingBufferArray[useIndexOfFrame].NetFrame >= checkFrame)
				{
					this.SetNext(useIndexOfFrame);
					return;
				}
				num += 1U;
			}
			this.SetNext(this.pastInterpolationIndex);
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x00019076 File Offset: 0x00017276
		public InterpolationRingBuffer(int ringCount)
			: base(ringCount)
		{
			this.searchCount = Mathf.Min(30, base.RingBufferArray.Length);
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0001909F File Offset: 0x0001729F
		public void InitPastAndNext(uint frame)
		{
			this.pastInterpolationIndex = base.GetUseIndexOfFrame(frame);
			this.nextInterpolationIndex = this.pastInterpolationIndex;
		}

		// Token: 0x040003E6 RID: 998
		private const uint MAX_SEARCH_COUNT = 30U;

		// Token: 0x040003E7 RID: 999
		public UnityEvent<T, T> OnStatesShifted = new UnityEvent<T, T>();

		// Token: 0x040003E8 RID: 1000
		public T ActiveInterpolatedState;

		// Token: 0x040003E9 RID: 1001
		private double _activeInterpolationTime;

		// Token: 0x040003EB RID: 1003
		private int pastInterpolationIndex;

		// Token: 0x040003EC RID: 1004
		private int nextInterpolationIndex;

		// Token: 0x040003ED RID: 1005
		private int searchCount;
	}
}
