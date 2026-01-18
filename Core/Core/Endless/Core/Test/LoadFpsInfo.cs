using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Endless.Core.Test
{
	// Token: 0x020000DF RID: 223
	public class LoadFpsInfo : BaseFpsInfo
	{
		// Token: 0x1700009F RID: 159
		// (get) Token: 0x06000509 RID: 1289 RVA: 0x00018527 File Offset: 0x00016727
		public override bool IsDone
		{
			get
			{
				return this.totalTime >= this.ShotTime;
			}
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x0001853A File Offset: 0x0001673A
		protected override void Awake()
		{
			base.Awake();
			this.StopTest();
		}

		// Token: 0x0600050B RID: 1291 RVA: 0x00018548 File Offset: 0x00016748
		public override void StartTest()
		{
			if (this.VirtualCamera)
			{
				this.VirtualCamera.enabled = true;
			}
		}

		// Token: 0x0600050C RID: 1292 RVA: 0x00018563 File Offset: 0x00016763
		public override void StopTest()
		{
			if (this.VirtualCamera)
			{
				this.VirtualCamera.enabled = false;
			}
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x0001857E File Offset: 0x0001677E
		protected override void ProcessFrame_Internal()
		{
			this.totalTime += Time.deltaTime;
		}

		// Token: 0x0400036B RID: 875
		public CinemachineVirtualCamera VirtualCamera;

		// Token: 0x0400036C RID: 876
		public float ShotTime = 3f;

		// Token: 0x0400036D RID: 877
		private float totalTime;
	}
}
