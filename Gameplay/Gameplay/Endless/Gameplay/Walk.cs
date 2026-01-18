using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x020001AE RID: 430
	public class Walk : Motion
	{
		// Token: 0x06000993 RID: 2451 RVA: 0x0002BE96 File Offset: 0x0002A096
		protected Walk(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
			: base(entity, segment, lookRotationOverride)
		{
		}

		// Token: 0x06000994 RID: 2452 RVA: 0x0002BEAC File Offset: 0x0002A0AC
		public override bool CanRun()
		{
			return NavMesh.CalculatePath(this.entity.FootPosition, this.segment.EndPosition, this.entity.Components.Agent.areaMask, this.path);
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x0002BEEC File Offset: 0x0002A0EC
		public override bool IsComplete()
		{
			return !this.entity.Components.Agent.pathPending && this.entity.Components.Agent.isOnNavMesh && this.entity.Components.Agent.remainingDistance <= this.entity.Components.Agent.stoppingDistance;
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x0002BF58 File Offset: 0x0002A158
		public override bool HasFailed()
		{
			return this.failedToNavigate;
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public override void WriteState(ref NpcState state)
		{
		}

		// Token: 0x06000998 RID: 2456 RVA: 0x0002BF60 File Offset: 0x0002A160
		public override IEnumerator Execute()
		{
			this.entity.Components.Agent.updateRotation = !this.lookRotationOverride;
			if (!this.entity.Components.Agent.SetPath(this.path) && !this.entity.Components.Agent.SetDestination(this.segment.EndPosition))
			{
				this.failedToNavigate = true;
				yield break;
			}
			if (this.lookRotationOverride)
			{
				while (this.lookRotationOverride)
				{
					Vector3 vector = this.lookRotationOverride.position - this.entity.transform.position;
					vector = new Vector3(vector.x, 0f, vector.z);
					Quaternion quaternion = Quaternion.LookRotation(vector);
					if (Quaternion.Angle(this.entity.transform.rotation, quaternion) > 5f)
					{
						this.entity.transform.rotation = Quaternion.RotateTowards(this.entity.transform.rotation, quaternion, Time.deltaTime * this.entity.Settings.RotationSpeed);
					}
					yield return null;
				}
			}
			yield break;
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x0002BF70 File Offset: 0x0002A170
		public override void Stop()
		{
			this.entity.Components.Agent.updateRotation = false;
			if (this.entity.Components.Agent.isOnNavMesh)
			{
				this.entity.Components.Agent.ResetPath();
			}
		}

		// Token: 0x040007BE RID: 1982
		private readonly NavMeshPath path = new NavMeshPath();

		// Token: 0x040007BF RID: 1983
		private bool failedToNavigate;
	}
}
