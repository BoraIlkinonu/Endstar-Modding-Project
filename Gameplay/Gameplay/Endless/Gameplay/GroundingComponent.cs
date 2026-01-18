using System;
using Endless.Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000134 RID: 308
	public class GroundingComponent
	{
		// Token: 0x06000724 RID: 1828 RVA: 0x00021F7D File Offset: 0x0002017D
		public GroundingComponent(NavMeshAgent agent, IndividualStateUpdater stateUpdater)
		{
			this.agent = agent;
			stateUpdater.OnCheckGroundState += this.HandleOnUpdateState;
			this.LastGroundedFrame = NetClock.CurrentFrame;
		}

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000725 RID: 1829 RVA: 0x00021FB0 File Offset: 0x000201B0
		public bool IsForcedUnground
		{
			get
			{
				return NetClock.CurrentFrame <= this.forcedUngroundFrame + 5U;
			}
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x00021FC4 File Offset: 0x000201C4
		public void ForceUnground()
		{
			this.forcedUngroundFrame = NetClock.CurrentFrame;
			this.IsGrounded = false;
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x00021FD8 File Offset: 0x000201D8
		private void HandleOnUpdateState()
		{
			bool flag = NetClock.CurrentFrame <= this.forcedUngroundFrame + 5U;
			Vector3 vector = this.agent.transform.position - new Vector3(0f, this.agent.baseOffset, 0f);
			if (flag)
			{
				this.IsGrounded = false;
				return;
			}
			NavMeshHit navMeshHit;
			this.IsGrounded = this.agent.isOnNavMesh || NavMesh.SamplePosition(vector, out navMeshHit, 0.1f, -1);
			if (this.IsGrounded)
			{
				this.LastGroundedFrame = NetClock.CurrentFrame;
			}
		}

		// Token: 0x040005B9 RID: 1465
		[ShowOnly]
		public bool IsGrounded = true;

		// Token: 0x040005BA RID: 1466
		[ShowOnly]
		public uint LastGroundedFrame;

		// Token: 0x040005BB RID: 1467
		private readonly NavMeshAgent agent;

		// Token: 0x040005BC RID: 1468
		private const uint ungroundFrames = 5U;

		// Token: 0x040005BD RID: 1469
		private uint forcedUngroundFrame;
	}
}
