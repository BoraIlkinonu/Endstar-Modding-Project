using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200033C RID: 828
	public class SentryTurretTrackLaserAppearance : MonoBehaviour
	{
		// Token: 0x060013E6 RID: 5094 RVA: 0x0006054A File Offset: 0x0005E74A
		public void Init(LineRenderer lr, LayerMask layerMask)
		{
			this.trackLineRenderer = lr;
			this.scanHitLayerMask = layerMask;
		}

		// Token: 0x060013E7 RID: 5095 RVA: 0x0006055A File Offset: 0x0005E75A
		private void Start()
		{
			this.trackLineRenderer.enabled = false;
		}

		// Token: 0x060013E8 RID: 5096 RVA: 0x00060568 File Offset: 0x0005E768
		public void SetState(bool active, float distance)
		{
			this.shootDistance = distance;
			if (active && !this.currentlyActive)
			{
				base.Invoke("EnableTrackLineRenderer", this.aimLaserDelay);
			}
			else if (!active)
			{
				this.trackLineRenderer.enabled = false;
			}
			this.currentlyActive = active;
		}

		// Token: 0x060013E9 RID: 5097 RVA: 0x000605A8 File Offset: 0x0005E7A8
		public void SetState(SentryTurretTrackLaserAppearance.State newState)
		{
			if (this.currentState == newState)
			{
				return;
			}
			switch (newState)
			{
			case SentryTurretTrackLaserAppearance.State.Inactive:
				this.trackLineRenderer.enabled = false;
				break;
			case SentryTurretTrackLaserAppearance.State.AimLaser:
				base.Invoke("EnableTrackLineRenderer", this.aimLaserDelay);
				break;
			case SentryTurretTrackLaserAppearance.State.ShootLaser:
				this.trackLineRenderer.enabled = false;
				break;
			}
			this.currentState = newState;
		}

		// Token: 0x060013EA RID: 5098 RVA: 0x00060607 File Offset: 0x0005E807
		private void EnableTrackLineRenderer()
		{
			if (this.currentlyActive)
			{
				this.trackLineRenderer.enabled = true;
			}
		}

		// Token: 0x060013EB RID: 5099 RVA: 0x00060620 File Offset: 0x0005E820
		public void Update()
		{
			this.ray = new Ray(base.transform.position, base.transform.forward);
			RaycastHit raycastHit;
			if (Physics.Raycast(base.transform.position, base.transform.forward, out raycastHit, this.shootDistance, this.scanHitLayerMask))
			{
				this.trackLineRenderer.SetPosition(1, Vector3.forward * raycastHit.distance);
				return;
			}
			this.trackLineRenderer.SetPosition(1, Vector3.forward * this.shootDistance);
		}

		// Token: 0x040010AC RID: 4268
		[Header("References")]
		[SerializeField]
		private LineRenderer trackLineRenderer;

		// Token: 0x040010AD RID: 4269
		[SerializeField]
		private LayerMask scanHitLayerMask;

		// Token: 0x040010AE RID: 4270
		private float aimLaserDelay = 0.1f;

		// Token: 0x040010AF RID: 4271
		private float shootDistance;

		// Token: 0x040010B0 RID: 4272
		private bool currentlyActive;

		// Token: 0x040010B1 RID: 4273
		private SentryTurretTrackLaserAppearance.State currentState;

		// Token: 0x040010B2 RID: 4274
		private Ray ray;

		// Token: 0x0200033D RID: 829
		public enum State
		{
			// Token: 0x040010B4 RID: 4276
			Inactive,
			// Token: 0x040010B5 RID: 4277
			AimLaser,
			// Token: 0x040010B6 RID: 4278
			ShootLaser
		}
	}
}
