using System;
using UnityEngine;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x0200043D RID: 1085
	public class StumbleState : FsmState
	{
		// Token: 0x1700057B RID: 1403
		// (get) Token: 0x06001B28 RID: 6952 RVA: 0x0004FDF8 File Offset: 0x0004DFF8
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Stumble;
			}
		}

		// Token: 0x06001B29 RID: 6953 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public StumbleState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x06001B2A RID: 6954 RVA: 0x0007B728 File Offset: 0x00079928
		public override void Enter()
		{
			base.Enter();
			base.Components.CharacterController.enableOverlapRecovery = true;
			base.Components.Animator.SetTrigger(NpcAnimator.SmallPush);
			base.Entity.Components.IndividualStateUpdater.GetCurrentState().SmallPush = true;
			this.isStopping = false;
		}

		// Token: 0x06001B2B RID: 6955 RVA: 0x0007B783 File Offset: 0x00079983
		protected override void WriteState(ref NpcState npcState)
		{
			if (this.loopTrigger)
			{
				npcState.LoopSmallPush = true;
				this.loopTrigger = false;
			}
			if (this.endTrigger)
			{
				npcState.EndSmallPush = true;
				this.endTrigger = false;
			}
		}

		// Token: 0x06001B2C RID: 6956 RVA: 0x0007B7B4 File Offset: 0x000799B4
		protected override void Update()
		{
			base.Update();
			Vector3 currentPhysics = base.Components.PhysicsTaker.CurrentPhysics;
			base.Components.CharacterController.Move(currentPhysics * Time.deltaTime);
			if (currentPhysics.magnitude < 0.4f && !this.isStopping)
			{
				this.isStopping = true;
				base.Components.Animator.SetTrigger(NpcAnimator.EndSmallPush);
				this.endTrigger = true;
			}
			if (currentPhysics.magnitude > 0.4f && this.isStopping)
			{
				this.isStopping = false;
				base.Components.Animator.SetTrigger(NpcAnimator.LoopSmallPush);
				this.loopTrigger = true;
			}
		}

		// Token: 0x06001B2D RID: 6957 RVA: 0x0007B868 File Offset: 0x00079A68
		protected override void Exit()
		{
			base.Exit();
			base.Components.CharacterController.enableOverlapRecovery = false;
			base.Components.Animator.SetTrigger(NpcAnimator.PhysicsForceExit);
			base.Entity.Components.IndividualStateUpdater.GetCurrentState().PhysicsForceExit = true;
		}

		// Token: 0x0400158E RID: 5518
		private const float STOPPING_THRESHOLD = 0.4f;

		// Token: 0x0400158F RID: 5519
		private bool isStopping;

		// Token: 0x04001590 RID: 5520
		private bool loopTrigger;

		// Token: 0x04001591 RID: 5521
		private bool endTrigger;
	}
}
