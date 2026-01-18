using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x0200043C RID: 1084
	public class StandUpState : FsmState
	{
		// Token: 0x1700057A RID: 1402
		// (get) Token: 0x06001B1F RID: 6943 RVA: 0x0001B97C File Offset: 0x00019B7C
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.StandUp;
			}
		}

		// Token: 0x06001B20 RID: 6944 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public StandUpState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x06001B21 RID: 6945 RVA: 0x0007B4A8 File Offset: 0x000796A8
		public override void Enter()
		{
			base.Enter();
			base.Components.Animator.SetFloat(NpcAnimator.FallTime, 1.5f);
			base.Components.Animator.SetTrigger(NpcAnimator.Landed);
			ref NpcState currentState = ref base.Entity.Components.IndividualStateUpdater.GetCurrentState();
			currentState.fallTime = 1.5f;
			currentState.landed = true;
			base.Components.Parameters.WillSplat = false;
			this.exitFrame = NetClock.CurrentFrame + 65U;
			base.Entity.Components.IndividualStateUpdater.OnTickAi += this.HandleOnTickAi;
		}

		// Token: 0x06001B22 RID: 6946 RVA: 0x0007B550 File Offset: 0x00079750
		protected override void Exit()
		{
			base.Exit();
			base.Entity.Components.IndividualStateUpdater.OnTickAi -= this.HandleOnTickAi;
			base.Components.Animator.ResetTrigger(NpcAnimator.Landed);
		}

		// Token: 0x06001B23 RID: 6947 RVA: 0x0007B58E File Offset: 0x0007978E
		private void HandleOnTickAi()
		{
			if (NetClock.CurrentFrame >= this.exitFrame)
			{
				if (!base.Components.Agent.enabled)
				{
					base.Components.Agent.enabled = true;
					return;
				}
				this.SetAppropriateTrigger();
			}
		}

		// Token: 0x06001B24 RID: 6948 RVA: 0x0007B5C8 File Offset: 0x000797C8
		private void SetAppropriateTrigger()
		{
			if (base.Entity.Components.Grounding.IsGrounded)
			{
				base.Components.Parameters.StandUpCompleteTrigger = true;
				return;
			}
			Vector3 vector;
			if (StandUpState.CanJumpToNavMesh(base.Entity, out vector))
			{
				base.Components.Parameters.JumpTrigger = true;
				base.Entity.NpcBlackboard.Set<Vector3>(NpcBlackboard.Key.JumpPosition, vector);
				return;
			}
			if (MonoBehaviourSingleton<Pathfinding>.Instance.NumSections > 0)
			{
				base.Components.Parameters.WarpTrigger = true;
			}
		}

		// Token: 0x06001B25 RID: 6949 RVA: 0x0007B650 File Offset: 0x00079850
		private static bool CanJumpToNavMesh(NpcEntity entity, out Vector3 position)
		{
			using (List<Vector3>.Enumerator enumerator = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(entity.FootPosition, 5f).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NavMeshHit navMeshHit;
					if (NavMesh.SamplePosition(enumerator.Current + Vector3.up * 0.5f, out navMeshHit, 1f, -1) && Physics.OverlapSphereNonAlloc(navMeshHit.position, 0.1f, StandUpState.colliders, entity.Settings.CharacterCollisionMask) <= 0)
					{
						position = navMeshHit.position;
						return true;
					}
				}
			}
			position = Vector3.zero;
			return false;
		}

		// Token: 0x06001B26 RID: 6950 RVA: 0x0007B20A File Offset: 0x0007940A
		protected override void Update()
		{
			base.Update();
			base.Components.CharacterController.SimpleMove(Vector3.zero);
		}

		// Token: 0x0400158C RID: 5516
		private uint exitFrame;

		// Token: 0x0400158D RID: 5517
		private static readonly Collider[] colliders = new Collider[5];
	}
}
