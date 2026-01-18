using System;

namespace Endless.Gameplay
{
	// Token: 0x02000373 RID: 883
	public abstract class Timer
	{
		// Token: 0x170004C0 RID: 1216
		// (get) Token: 0x060016A0 RID: 5792 RVA: 0x0006A861 File Offset: 0x00068A61
		// (set) Token: 0x060016A1 RID: 5793 RVA: 0x0006A869 File Offset: 0x00068A69
		public float Time { get; set; }

		// Token: 0x170004C1 RID: 1217
		// (get) Token: 0x060016A2 RID: 5794 RVA: 0x0006A872 File Offset: 0x00068A72
		// (set) Token: 0x060016A3 RID: 5795 RVA: 0x0006A87A File Offset: 0x00068A7A
		public bool IsRunning { get; protected set; }

		// Token: 0x170004C2 RID: 1218
		// (get) Token: 0x060016A4 RID: 5796 RVA: 0x0006A883 File Offset: 0x00068A83
		public float Progress
		{
			get
			{
				return this.Time / this.initialTime;
			}
		}

		// Token: 0x060016A5 RID: 5797 RVA: 0x0006A894 File Offset: 0x00068A94
		protected Timer(float value)
		{
			this.initialTime = value;
			this.IsRunning = false;
		}

		// Token: 0x060016A6 RID: 5798 RVA: 0x0006A8FF File Offset: 0x00068AFF
		public void Start()
		{
			this.Time = this.initialTime;
			if (!this.IsRunning)
			{
				this.IsRunning = true;
				this.OnTimerStart();
			}
		}

		// Token: 0x060016A7 RID: 5799 RVA: 0x0006A927 File Offset: 0x00068B27
		public void Stop()
		{
			if (this.IsRunning)
			{
				this.IsRunning = false;
				this.OnTimerStop();
			}
		}

		// Token: 0x060016A8 RID: 5800 RVA: 0x0006A943 File Offset: 0x00068B43
		public void Resume()
		{
			this.IsRunning = true;
		}

		// Token: 0x060016A9 RID: 5801 RVA: 0x0006A94C File Offset: 0x00068B4C
		public void Pause()
		{
			this.IsRunning = false;
		}

		// Token: 0x060016AA RID: 5802
		public abstract void Tick(float deltaTime);

		// Token: 0x0400123D RID: 4669
		protected float initialTime;

		// Token: 0x04001240 RID: 4672
		public Action OnTimerStart = delegate
		{
		};

		// Token: 0x04001241 RID: 4673
		public Action OnTimerStop = delegate
		{
		};
	}
}
