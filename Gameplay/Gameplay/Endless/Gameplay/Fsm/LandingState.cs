using System;
using UnityEngine;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000439 RID: 1081
	public class LandingState : FsmState
	{
		// Token: 0x06001B0F RID: 6927 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public LandingState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x17000577 RID: 1399
		// (get) Token: 0x06001B10 RID: 6928 RVA: 0x0007B18F File Offset: 0x0007938F
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Landing;
			}
		}

		// Token: 0x06001B11 RID: 6929 RVA: 0x0007B194 File Offset: 0x00079394
		public override void Enter()
		{
			base.Enter();
			base.Entity.Components.Animator.SetTrigger(NpcAnimator.Landed);
			base.Entity.Components.IndividualStateUpdater.GetCurrentState().landed = true;
			this.exitFrame = NetClock.CurrentFrame + 4U;
			base.Entity.Components.IndividualStateUpdater.OnTickAi += this.HandleOnTickAi;
		}

		// Token: 0x06001B12 RID: 6930 RVA: 0x0007B20A File Offset: 0x0007940A
		protected override void Update()
		{
			base.Update();
			base.Components.CharacterController.SimpleMove(Vector3.zero);
		}

		// Token: 0x06001B13 RID: 6931 RVA: 0x0007B228 File Offset: 0x00079428
		private void HandleOnTickAi()
		{
			if (NetClock.CurrentFrame >= this.exitFrame)
			{
				base.Components.Parameters.LandingCompleteTrigger = true;
			}
		}

		// Token: 0x06001B14 RID: 6932 RVA: 0x0007B248 File Offset: 0x00079448
		protected override void Exit()
		{
			base.Exit();
			base.Entity.Components.IndividualStateUpdater.OnTickAi -= this.HandleOnTickAi;
			base.Components.Animator.ResetTrigger(NpcAnimator.Landed);
		}

		// Token: 0x0400158B RID: 5515
		private uint exitFrame;
	}
}
