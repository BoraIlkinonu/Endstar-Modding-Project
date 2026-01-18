using System;

namespace Endless.Gameplay
{
	// Token: 0x02000376 RID: 886
	public class StopwatchTimer : Timer
	{
		// Token: 0x060016B5 RID: 5813 RVA: 0x0006A9E1 File Offset: 0x00068BE1
		public StopwatchTimer()
			: base(0f)
		{
		}

		// Token: 0x060016B6 RID: 5814 RVA: 0x0006A9EE File Offset: 0x00068BEE
		public override void Tick(float deltaTime)
		{
			if (base.IsRunning)
			{
				base.Time += deltaTime;
			}
		}

		// Token: 0x060016B7 RID: 5815 RVA: 0x0006AA06 File Offset: 0x00068C06
		public void Reset()
		{
			base.Time = 0f;
		}

		// Token: 0x060016B8 RID: 5816 RVA: 0x0006AA13 File Offset: 0x00068C13
		public float GetTime()
		{
			return base.Time;
		}
	}
}
