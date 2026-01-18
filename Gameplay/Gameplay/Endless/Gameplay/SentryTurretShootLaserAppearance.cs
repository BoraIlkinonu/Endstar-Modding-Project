using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200033B RID: 827
	public class SentryTurretShootLaserAppearance : MonoBehaviour
	{
		// Token: 0x060013E0 RID: 5088 RVA: 0x000604E3 File Offset: 0x0005E6E3
		public void Init(LineRenderer lr)
		{
			this.lineRenderer = lr;
		}

		// Token: 0x060013E1 RID: 5089 RVA: 0x000604EC File Offset: 0x0005E6EC
		private void OnDisable()
		{
			this.Stop();
		}

		// Token: 0x060013E2 RID: 5090 RVA: 0x000604F4 File Offset: 0x0005E6F4
		public void Play(Vector3 hitPosition, float duration)
		{
			this.lineRenderer.SetPosition(1, base.transform.InverseTransformPoint(hitPosition));
			this.lineRenderer.enabled = true;
			this.stopTime = Time.realtimeSinceStartup + duration;
		}

		// Token: 0x060013E3 RID: 5091 RVA: 0x00060527 File Offset: 0x0005E727
		public void Stop()
		{
			this.lineRenderer.enabled = false;
		}

		// Token: 0x060013E4 RID: 5092 RVA: 0x00060535 File Offset: 0x0005E735
		private void Update()
		{
			if (Time.realtimeSinceStartup > this.stopTime)
			{
				this.Stop();
			}
		}

		// Token: 0x040010AA RID: 4266
		[SerializeField]
		private LineRenderer lineRenderer;

		// Token: 0x040010AB RID: 4267
		private float stopTime;
	}
}
