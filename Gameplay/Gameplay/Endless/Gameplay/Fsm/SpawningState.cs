using System;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x0200043B RID: 1083
	public class SpawningState : FsmState
	{
		// Token: 0x06001B1C RID: 6940 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public SpawningState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x17000579 RID: 1401
		// (get) Token: 0x06001B1D RID: 6941 RVA: 0x0007B485 File Offset: 0x00079685
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Spawning;
			}
		}

		// Token: 0x06001B1E RID: 6942 RVA: 0x0007B489 File Offset: 0x00079689
		public override void Enter()
		{
			base.Enter();
			base.Entity.Components.NpcAnimator.PlaySpawnAnimation();
		}
	}
}
