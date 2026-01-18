using System;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000432 RID: 1074
	public class DownedState : FsmState
	{
		// Token: 0x1700056C RID: 1388
		// (get) Token: 0x06001AE5 RID: 6885 RVA: 0x0001BD04 File Offset: 0x00019F04
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Downed;
			}
		}

		// Token: 0x06001AE6 RID: 6886 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public DownedState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x06001AE7 RID: 6887 RVA: 0x0007AA00 File Offset: 0x00078C00
		public override void Enter()
		{
			base.Enter();
			base.Entity.ExplosionsOnlyClientRpc();
			base.Entity.DownedClientRpc();
			base.Entity.Components.Animator.SetTrigger(NpcAnimator.Downed);
			base.Entity.Components.Animator.SetBool(NpcAnimator.Dbno, true);
		}
	}
}
