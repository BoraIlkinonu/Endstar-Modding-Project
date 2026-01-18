using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200035D RID: 861
	public class TargeterManager : EndlessBehaviourSingleton<TargeterManager>, NetClock.ISimulateFrameLateSubscriber
	{
		// Token: 0x170004A1 RID: 1185
		// (get) Token: 0x060015EF RID: 5615 RVA: 0x0006802F File Offset: 0x0006622F
		// (set) Token: 0x060015F0 RID: 5616 RVA: 0x00068037 File Offset: 0x00066237
		public int TickOffsetRange
		{
			get
			{
				return this.tickOffsetRange;
			}
			private set
			{
				if (this.tickOffsetRange == value)
				{
					return;
				}
				this.tickOffsetRange = value;
				this.UpdateNpcTickOffset(this.tickOffsetRange);
			}
		}

		// Token: 0x060015F1 RID: 5617 RVA: 0x00068056 File Offset: 0x00066256
		protected override void Awake()
		{
			base.Awake();
			NetClock.Register(this);
		}

		// Token: 0x060015F2 RID: 5618 RVA: 0x00068064 File Offset: 0x00066264
		protected override void OnDestroy()
		{
			base.OnDestroy();
			NetClock.Unregister(this);
		}

		// Token: 0x060015F3 RID: 5619 RVA: 0x00068072 File Offset: 0x00066272
		public void RegisterTargeter(TargeterComponent targeter)
		{
			this.targeters.Add(targeter);
		}

		// Token: 0x060015F4 RID: 5620 RVA: 0x00068080 File Offset: 0x00066280
		public void UnregisterTargeter(TargeterComponent targeter)
		{
			this.targeters.Remove(targeter);
		}

		// Token: 0x060015F5 RID: 5621 RVA: 0x00068090 File Offset: 0x00066290
		private void UpdateNpcTickOffset(int offsetRange)
		{
			if (offsetRange == 1)
			{
				this.targeters.ForEach(delegate(TargeterComponent targeter)
				{
					targeter.TickOffset = 0;
				});
				return;
			}
			int num = 0;
			for (int i = 0; i < this.targeters.Count; i++)
			{
				this.targeters[i].TickOffset = num++;
				if (num >= offsetRange)
				{
					num = 0;
				}
			}
		}

		// Token: 0x060015F6 RID: 5622 RVA: 0x00068100 File Offset: 0x00066300
		public void SimulateFrameLate(uint frame)
		{
			this.TickOffsetRange = Mathf.Clamp(this.targeters.Count / 20, 1, 10);
		}

		// Token: 0x040011E9 RID: 4585
		private const int MAX_TICK_OFFSETS = 10;

		// Token: 0x040011EA RID: 4586
		private const int GOAL_TARGETERS_PER_TICK = 20;

		// Token: 0x040011EB RID: 4587
		private readonly List<TargeterComponent> targeters = new List<TargeterComponent>();

		// Token: 0x040011EC RID: 4588
		private int tickOffsetRange = 1;
	}
}
