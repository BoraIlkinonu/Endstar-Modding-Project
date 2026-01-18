using System;
using UnityEngine;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000436 RID: 1078
	public class GroundedPhysicsState : FsmState
	{
		// Token: 0x06001B03 RID: 6915 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public GroundedPhysicsState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x17000574 RID: 1396
		// (get) Token: 0x06001B04 RID: 6916 RVA: 0x0001BCEF File Offset: 0x00019EEF
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.GroundedPhysics;
			}
		}

		// Token: 0x06001B05 RID: 6917 RVA: 0x0007AFF6 File Offset: 0x000791F6
		public override void Enter()
		{
			base.Enter();
			base.Entity.Components.IndividualStateUpdater.GetCurrentState().isGrounded = true;
			base.Entity.NpcBlackboard.Set<float>(NpcBlackboard.Key.OverrideSpeed, 1f);
		}

		// Token: 0x06001B06 RID: 6918 RVA: 0x0007B030 File Offset: 0x00079230
		protected override void Update()
		{
			base.Update();
			Vector3 currentPhysics = base.Components.PhysicsTaker.CurrentPhysics;
			base.Components.CharacterController.SimpleMove(currentPhysics);
		}

		// Token: 0x06001B07 RID: 6919 RVA: 0x0007B066 File Offset: 0x00079266
		protected override void WriteState(ref NpcState npcState)
		{
			npcState.isGrounded = true;
		}

		// Token: 0x06001B08 RID: 6920 RVA: 0x0007B070 File Offset: 0x00079270
		protected override void Exit()
		{
			base.Exit();
			base.Components.Animator.SetTrigger(NpcAnimator.PhysicsForceExit);
			base.Entity.Components.IndividualStateUpdater.GetCurrentState().PhysicsForceExit = true;
			base.Entity.NpcBlackboard.Clear<float>(NpcBlackboard.Key.OverrideSpeed);
		}

		// Token: 0x04001589 RID: 5513
		private float cachedSpeed;
	}
}
