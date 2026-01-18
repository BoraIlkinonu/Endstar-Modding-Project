using System;
using UnityEngine;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000434 RID: 1076
	public class FreeFallState : FsmState
	{
		// Token: 0x1700056E RID: 1390
		// (get) Token: 0x06001AED RID: 6893 RVA: 0x0003FE71 File Offset: 0x0003E071
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.FreeFall;
			}
		}

		// Token: 0x06001AEE RID: 6894 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public FreeFallState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x06001AEF RID: 6895 RVA: 0x0007AB08 File Offset: 0x00078D08
		public override void Enter()
		{
			base.Enter();
			this.entryTrigger = true;
			this.fallTime = 0f;
			this.isInFreeFall = false;
			base.Components.CharacterController.enableOverlapRecovery = true;
			base.Components.Animator.ResetTrigger(NpcAnimator.PhysicsForceExit);
			base.Components.Animator.SetTrigger(NpcAnimator.LargePush);
			base.Components.Animator.SetBool(NpcAnimator.Grounded, false);
			base.Components.IndividualStateUpdater.OnUpdateState += this.HandleOnUpdateState;
		}

		// Token: 0x06001AF0 RID: 6896 RVA: 0x0007ABA4 File Offset: 0x00078DA4
		private void HandleOnUpdateState(uint obj)
		{
			Vector3 forcesPlusGravity = this.GetForcesPlusGravity();
			if (forcesPlusGravity.y > 0f)
			{
				return;
			}
			base.Components.Parameters.WillRoll = forcesPlusGravity.normalized.y > -0.5f;
			base.Components.Parameters.WillSplat = this.fallTime > 1.4f;
			if (!base.Components.Parameters.WillRoll && !this.isInFreeFall)
			{
				base.Components.Animator.SetTrigger(NpcAnimator.PhysicsForceExit);
				this.isInFreeFall = true;
				this.freeFallTrigger = true;
			}
		}

		// Token: 0x06001AF1 RID: 6897 RVA: 0x0007AC44 File Offset: 0x00078E44
		protected override void WriteState(ref NpcState npcState)
		{
			if (this.entryTrigger)
			{
				npcState.LargePush = true;
				this.entryTrigger = false;
			}
			if (this.freeFallTrigger)
			{
				npcState.PhysicsForceExit = true;
				this.freeFallTrigger = false;
			}
			npcState.isGrounded = false;
			npcState.fallTime = this.fallTime;
		}

		// Token: 0x06001AF2 RID: 6898 RVA: 0x0007AC90 File Offset: 0x00078E90
		protected override void Update()
		{
			base.Update();
			Vector3 vector = this.GetForcesPlusGravity();
			if ((double)vector.normalized.y < -0.8)
			{
				Vector3 vector2 = base.Entity.transform.forward * 2f;
				vector += vector2;
			}
			base.Components.CharacterController.Move(vector * Time.deltaTime);
			if (vector.y < 0f)
			{
				this.fallTime += Time.deltaTime;
			}
			base.Components.Animator.SetFloat(NpcAnimator.FallTime, this.fallTime);
		}

		// Token: 0x06001AF3 RID: 6899 RVA: 0x0007AD3C File Offset: 0x00078F3C
		private Vector3 GetForcesPlusGravity()
		{
			Vector3 currentPhysics = base.Components.PhysicsTaker.CurrentPhysics;
			uint num = NetClock.CurrentFrame - base.Components.Grounding.LastGroundedFrame;
			float num2 = Mathf.InverseLerp(0f, (float)base.Entity.Settings.FramesToTerminalVelocity, num);
			float num3 = base.Entity.Settings.GravityCurve.Evaluate(num2);
			currentPhysics.y -= base.Entity.Settings.TerminalVelocity * num3;
			return currentPhysics;
		}

		// Token: 0x06001AF4 RID: 6900 RVA: 0x0007ADC8 File Offset: 0x00078FC8
		protected override void Exit()
		{
			base.Exit();
			base.Components.CharacterController.enableOverlapRecovery = false;
			base.Components.Animator.SetBool(NpcAnimator.Grounded, true);
			base.Components.IndividualStateUpdater.OnUpdateState -= this.HandleOnUpdateState;
		}

		// Token: 0x04001581 RID: 5505
		private const float rollThreshold = 0.5f;

		// Token: 0x04001582 RID: 5506
		private const float splatThreshold = 1.4f;

		// Token: 0x04001583 RID: 5507
		private float fallTime;

		// Token: 0x04001584 RID: 5508
		private bool isInFreeFall;

		// Token: 0x04001585 RID: 5509
		private bool entryTrigger;

		// Token: 0x04001586 RID: 5510
		private bool freeFallTrigger;
	}
}
