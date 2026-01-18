using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x020001B2 RID: 434
	public class Jump : Motion
	{
		// Token: 0x060009AF RID: 2479 RVA: 0x0002C6C8 File Offset: 0x0002A8C8
		protected Jump(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
			: base(entity, segment, lookRotationOverride)
		{
			NavMeshHit navMeshHit;
			if (!NavMesh.SamplePosition(entity.FootPosition, out navMeshHit, 1f, -1))
			{
				this.canRun = false;
				return;
			}
			if (!NavMesh.SamplePosition(segment.EndPosition, out navMeshHit, 1f, -1))
			{
				this.canRun = false;
				return;
			}
			this.endPosition = navMeshHit.position;
			float3 @float = entity.FootPosition;
			float num = math.clamp(BurstPathfindingUtilities.EstimateLaunchAngle(in @float, in this.endPosition), 5f, 85f);
			@float = entity.FootPosition;
			if (!BurstPathfindingUtilities.CanReachJumpPosition(in @float, in this.endPosition, NpcMovementValues.MaxVerticalVelocity * 1.2f, NpcMovementValues.MaxHorizontalVelocity * 1.2f, NpcMovementValues.Gravity))
			{
				this.canRun = false;
				return;
			}
			@float = entity.FootPosition;
			if (!BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in @float, in this.endPosition, num, NpcMovementValues.Gravity, out this.initialVelocity, out this.timeOfFlight) || float.IsNaN(this.timeOfFlight))
			{
				this.canRun = false;
				return;
			}
			this.canRun = true;
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x0002C7DF File Offset: 0x0002A9DF
		public override bool CanRun()
		{
			return this.canRun;
		}

		// Token: 0x060009B1 RID: 2481 RVA: 0x0002C7E7 File Offset: 0x0002A9E7
		public override bool IsComplete()
		{
			return this.complete;
		}

		// Token: 0x060009B2 RID: 2482 RVA: 0x0001965C File Offset: 0x0001785C
		public override bool HasFailed()
		{
			return false;
		}

		// Token: 0x060009B3 RID: 2483 RVA: 0x0002C7F0 File Offset: 0x0002A9F0
		public override void WriteState(ref NpcState state)
		{
			if (this.jumpTrigger)
			{
				state.jumped = true;
				this.jumpTrigger = false;
			}
			if (this.landTrigger)
			{
				state.landed = true;
				this.landTrigger = false;
			}
			state.fallTime = this.fallingTime;
			state.isAirborne = true;
		}

		// Token: 0x060009B4 RID: 2484 RVA: 0x0002C83C File Offset: 0x0002AA3C
		public override IEnumerator Execute()
		{
			float time = 0f;
			this.entity.Components.GoapController.LockPlan = true;
			this.entity.Components.Agent.updatePosition = false;
			this.entity.Components.Animator.SetTrigger(NpcAnimator.Jump);
			this.jumpTrigger = true;
			this.entity.Components.Animator.SetBool(NpcAnimator.Grounded, false);
			float3 startPosition = this.entity.Position;
			Vector3 forward = this.segment.EndPosition.ToVector3() - this.entity.transform.position;
			forward = new Vector3(forward.x, 0f, forward.z);
			while (time < this.timeOfFlight)
			{
				this.entity.transform.rotation = Quaternion.RotateTowards(this.entity.transform.rotation, Quaternion.LookRotation(forward, Vector3.up), 180f * Time.deltaTime);
				Vector3 lastPosition = this.entity.Position;
				Vector3 position = BurstPathfindingUtilities.GetPointOnCurve(startPosition, this.initialVelocity, time, NpcMovementValues.Gravity);
				this.entity.Position = position;
				yield return null;
				time += Time.deltaTime;
				if (lastPosition.y > position.y)
				{
					this.fallingTime += Time.deltaTime;
				}
				lastPosition = default(Vector3);
				position = default(Vector3);
			}
			if (!Physics.Raycast(this.entity.Position, Vector3.down, 0.6f, LayerMask.GetMask(new string[] { "Default" })))
			{
				this.entity.Components.PhysicsTaker.TakePhysicsForce(0f, Vector3.down, NetClock.CurrentFrame, this.entity.NetworkObjectId, true, false, false);
				this.entity.Components.GoapController.LockPlan = false;
				this.complete = true;
			}
			else
			{
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(this.endPosition.ToVector3() + Vector3.up * 0.5f, out navMeshHit, 0.5f, -1))
				{
					this.entity.Components.Agent.Warp(navMeshHit.position);
					this.entity.Components.transform.position = navMeshHit.position + Vector3.up * 0.5f;
				}
				else
				{
					this.entity.Components.Agent.Warp(this.endPosition.ToVector3() + Vector3.up * 0.5f);
					this.entity.transform.position = this.endPosition.ToVector3() + Vector3.up * 0.5f;
				}
				this.entity.Components.Animator.SetBool(NpcAnimator.Grounded, true);
				this.entity.Components.Animator.SetTrigger(NpcAnimator.Landed);
				this.landTrigger = true;
				this.entity.Components.Agent.updatePosition = true;
				yield return new WaitForSeconds(0.1f);
				this.entity.Components.GoapController.LockPlan = false;
				this.complete = true;
			}
			yield break;
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x0002C84B File Offset: 0x0002AA4B
		public override void Stop()
		{
			this.entity.Components.GoapController.LockPlan = false;
		}

		// Token: 0x040007CD RID: 1997
		private readonly bool canRun;

		// Token: 0x040007CE RID: 1998
		private readonly float3 endPosition;

		// Token: 0x040007CF RID: 1999
		private readonly float3 initialVelocity;

		// Token: 0x040007D0 RID: 2000
		private readonly float timeOfFlight;

		// Token: 0x040007D1 RID: 2001
		private float fallingTime;

		// Token: 0x040007D2 RID: 2002
		private bool jumpTrigger;

		// Token: 0x040007D3 RID: 2003
		private bool landTrigger;

		// Token: 0x040007D4 RID: 2004
		private bool complete;
	}
}
