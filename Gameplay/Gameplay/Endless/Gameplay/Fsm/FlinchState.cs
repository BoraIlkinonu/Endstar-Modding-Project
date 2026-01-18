using System;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000433 RID: 1075
	public class FlinchState : FsmState
	{
		// Token: 0x1700056D RID: 1389
		// (get) Token: 0x06001AE8 RID: 6888 RVA: 0x0007AA5E File Offset: 0x00078C5E
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Flinch;
			}
		}

		// Token: 0x06001AE9 RID: 6889 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public FlinchState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x06001AEA RID: 6890 RVA: 0x0007AA64 File Offset: 0x00078C64
		public override void Enter()
		{
			base.Enter();
			this.exitFrame = NetClock.CurrentFrame + 5U;
			base.Entity.Components.Animator.SetTrigger(NpcAnimator.Flinch);
			base.Entity.Components.IndividualStateUpdater.OnTickAi += this.HandleOnTickAi;
		}

		// Token: 0x06001AEB RID: 6891 RVA: 0x0007AABF File Offset: 0x00078CBF
		protected override void Exit()
		{
			base.Exit();
			base.Entity.Components.IndividualStateUpdater.OnTickAi -= this.HandleOnTickAi;
		}

		// Token: 0x06001AEC RID: 6892 RVA: 0x0007AAE8 File Offset: 0x00078CE8
		private void HandleOnTickAi()
		{
			if (NetClock.CurrentFrame >= this.exitFrame)
			{
				base.Components.Parameters.FlinchFinishedTrigger = true;
			}
		}

		// Token: 0x04001580 RID: 5504
		private uint exitFrame;
	}
}
