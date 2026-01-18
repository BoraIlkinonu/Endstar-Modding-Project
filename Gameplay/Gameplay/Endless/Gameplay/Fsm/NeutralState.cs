using System;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x0200043A RID: 1082
	public class NeutralState : FsmState
	{
		// Token: 0x06001B15 RID: 6933 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public NeutralState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x17000578 RID: 1400
		// (get) Token: 0x06001B16 RID: 6934 RVA: 0x0007B286 File Offset: 0x00079486
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Neutral;
			}
		}

		// Token: 0x06001B17 RID: 6935 RVA: 0x0007B28C File Offset: 0x0007948C
		public override void Enter()
		{
			base.Enter();
			base.Components.IndividualStateUpdater.OnTickAi += this.HandleOnTickAi;
			base.Components.Agent.enabled = true;
			base.Components.Animator.SetBool(NpcAnimator.Grounded, true);
			base.Components.Animator.SetFloat(NpcAnimator.FallTime, 0f);
		}

		// Token: 0x06001B18 RID: 6936 RVA: 0x0007B2FC File Offset: 0x000794FC
		private void HandleOnTickAi()
		{
			base.Components.PlanFollower.Tick(NetClock.CurrentFrame);
			base.Components.GoapController.TickGoap(NetClock.CurrentFrame);
		}

		// Token: 0x06001B19 RID: 6937 RVA: 0x0007B328 File Offset: 0x00079528
		protected override void Update()
		{
			base.Update();
			base.Components.Animator.SetFloat(NpcAnimator.SlopeAngle, base.Components.SlopeComponent.SlopeAngle);
			base.Components.Animator.SetBool(NpcAnimator.Moving, base.Components.VelocityTracker.SmoothedSpeed > 0.2f);
			base.Components.PlanFollower.Update();
		}

		// Token: 0x06001B1A RID: 6938 RVA: 0x0007B39C File Offset: 0x0007959C
		protected override void WriteState(ref NpcState npcState)
		{
			npcState.slopeAngle = base.Components.SlopeComponent.SlopeAngle;
			npcState.isMoving = base.Components.VelocityTracker.SmoothedSpeed > 0.2f;
			npcState.isGrounded = !base.Components.PathFollower.IsJumping;
		}

		// Token: 0x06001B1B RID: 6939 RVA: 0x0007B3F8 File Offset: 0x000795F8
		protected override void Exit()
		{
			base.Exit();
			base.Components.IndividualStateUpdater.OnTickAi -= this.HandleOnTickAi;
			AttackComponent attack = base.Components.Attack;
			if (attack != null)
			{
				attack.ClearAttackQueue();
			}
			base.Components.GoapController.Uncontrolled();
			base.Components.PathFollower.StopPath(false);
			base.Components.Agent.enabled = false;
			base.Components.Animator.SetBool(NpcAnimator.Moving, false);
		}
	}
}
