using System;
using System.Collections.Generic;

namespace Endless.Gameplay
{
	// Token: 0x02000102 RID: 258
	public class MovingColliderManager : EndlessBehaviourSingleton<MovingColliderManager>, IStartSubscriber, IGameEndSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber
	{
		// Token: 0x060005C0 RID: 1472 RVA: 0x0001CAAE File Offset: 0x0001ACAE
		private void OnEnable()
		{
			NetClock.Register(this);
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x0001CAB6 File Offset: 0x0001ACB6
		private void OnDisable()
		{
			NetClock.Unregister(this);
		}

		// Token: 0x060005C2 RID: 1474 RVA: 0x0001CAC0 File Offset: 0x0001ACC0
		void NetClock.ISimulateFrameEnvironmentSubscriber.SimulateFrameEnvironment(uint frame)
		{
			if (this.playing)
			{
				foreach (MovingCollider movingCollider in this.movingCollidersThisFrame)
				{
					movingCollider.HandleMovementFrame();
				}
			}
		}

		// Token: 0x060005C3 RID: 1475 RVA: 0x0001CB18 File Offset: 0x0001AD18
		void IStartSubscriber.EndlessStart()
		{
			this.playing = true;
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x0001CB21 File Offset: 0x0001AD21
		void IGameEndSubscriber.EndlessGameEnd()
		{
			this.playing = false;
			this.movingCollidersThisFrame.Clear();
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x0001CB35 File Offset: 0x0001AD35
		public void ColliderMoved(MovingCollider movingCollider)
		{
			if (this.playing)
			{
				this.movingCollidersThisFrame.Add(movingCollider);
			}
		}

		// Token: 0x04000450 RID: 1104
		private HashSet<MovingCollider> movingCollidersThisFrame = new HashSet<MovingCollider>();

		// Token: 0x04000451 RID: 1105
		private bool playing;
	}
}
