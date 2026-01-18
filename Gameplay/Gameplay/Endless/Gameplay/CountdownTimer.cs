using System;

namespace Endless.Gameplay
{
	// Token: 0x02000375 RID: 885
	public class CountdownTimer : Timer
	{
		// Token: 0x060016AF RID: 5807 RVA: 0x0006A961 File Offset: 0x00068B61
		public CountdownTimer(float value)
			: base(value)
		{
		}

		// Token: 0x170004C3 RID: 1219
		// (get) Token: 0x060016B0 RID: 5808 RVA: 0x0006A96A File Offset: 0x00068B6A
		public bool IsFinished
		{
			get
			{
				return base.Time <= 0f;
			}
		}

		// Token: 0x170004C4 RID: 1220
		// (get) Token: 0x060016B1 RID: 5809 RVA: 0x0006A97C File Offset: 0x00068B7C
		public float CountdownDuration
		{
			get
			{
				return this.initialTime;
			}
		}

		// Token: 0x060016B2 RID: 5810 RVA: 0x0006A984 File Offset: 0x00068B84
		public override void Tick(float deltaTime)
		{
			if (base.IsRunning && base.Time > 0f)
			{
				base.Time -= deltaTime;
			}
			if (base.IsRunning && base.Time <= 0f)
			{
				base.Stop();
			}
		}

		// Token: 0x060016B3 RID: 5811 RVA: 0x0006A9C4 File Offset: 0x00068BC4
		public void Reset()
		{
			base.Time = this.initialTime;
		}

		// Token: 0x060016B4 RID: 5812 RVA: 0x0006A9D2 File Offset: 0x00068BD2
		public void Reset(float newTime)
		{
			this.initialTime = newTime;
			this.Reset();
		}
	}
}
