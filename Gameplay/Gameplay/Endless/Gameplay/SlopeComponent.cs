using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000151 RID: 337
	public class SlopeComponent
	{
		// Token: 0x060007F2 RID: 2034 RVA: 0x00025523 File Offset: 0x00023723
		public SlopeComponent(Transform transform, LayerMask mask, IndividualStateUpdater stateUpdater)
		{
			this.transform = transform;
			this.mask = mask;
			stateUpdater.OnCheckGroundState += this.HandleCheckGroundState;
		}

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x060007F3 RID: 2035 RVA: 0x0002554C File Offset: 0x0002374C
		public float SlopeAngle
		{
			get
			{
				Vector3 forward = this.transform.forward;
				float num = Vector3.Angle(this.groundNormal, forward);
				Vector2 vector = new Vector2(forward.x, forward.z);
				Vector2 vector2 = new Vector2(this.groundNormal.x, this.groundNormal.z);
				return Mathf.Abs(Vector2.Dot(vector.normalized, vector2.normalized)) * (num - 90f) / 45f;
			}
		}

		// Token: 0x060007F4 RID: 2036 RVA: 0x000255C8 File Offset: 0x000237C8
		private void HandleCheckGroundState()
		{
			RaycastHit raycastHit;
			Vector3 vector = (Physics.Raycast(this.transform.position, Vector3.down, out raycastHit, 1f, this.mask) ? raycastHit.normal : Vector3.up);
			this.groundNormal = Vector3.SmoothDamp(this.groundNormal, vector, ref this.normalVelocity, 0.2f, 90f * NetClock.FixedDeltaTime, NetClock.FixedDeltaTime);
		}

		// Token: 0x04000649 RID: 1609
		private readonly LayerMask mask;

		// Token: 0x0400064A RID: 1610
		private readonly Transform transform;

		// Token: 0x0400064B RID: 1611
		private Vector3 groundNormal;

		// Token: 0x0400064C RID: 1612
		private Vector3 normalVelocity;
	}
}
