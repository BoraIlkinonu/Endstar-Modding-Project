using System;

namespace Endless.Gameplay
{
	// Token: 0x020002DE RID: 734
	public struct ShootingState : IFrameInfo
	{
		// Token: 0x1700033B RID: 827
		// (get) Token: 0x06001099 RID: 4249 RVA: 0x00054209 File Offset: 0x00052409
		// (set) Token: 0x0600109A RID: 4250 RVA: 0x00054211 File Offset: 0x00052411
		public uint NetFrame
		{
			get
			{
				return this.netFrame;
			}
			set
			{
				if (value > this.netFrame)
				{
					this.netFrame = value;
				}
			}
		}

		// Token: 0x0600109B RID: 4251 RVA: 0x00054223 File Offset: 0x00052423
		public void Clear()
		{
			this.netFrame = 0U;
			this.finishedFrame = 0U;
			this.lastShotFrame = 0U;
			this.cameraLocked = false;
			this.commitToShot = false;
			this.shotReleased = true;
			this.waitingForShot = false;
			this.aimState = CameraController.CameraType.Normal;
		}

		// Token: 0x0600109C RID: 4252 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Initialize()
		{
		}

		// Token: 0x04000E3E RID: 3646
		private uint netFrame;

		// Token: 0x04000E3F RID: 3647
		public uint finishedFrame;

		// Token: 0x04000E40 RID: 3648
		public uint lastShotFrame;

		// Token: 0x04000E41 RID: 3649
		public bool cameraLocked;

		// Token: 0x04000E42 RID: 3650
		public bool commitToShot;

		// Token: 0x04000E43 RID: 3651
		public bool shotReleased;

		// Token: 0x04000E44 RID: 3652
		public bool waitingForShot;

		// Token: 0x04000E45 RID: 3653
		public CameraController.CameraType aimState;
	}
}
