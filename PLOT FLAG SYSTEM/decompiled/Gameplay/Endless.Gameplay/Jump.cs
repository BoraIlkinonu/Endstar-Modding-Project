using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class Jump : Motion
{
	private readonly bool canRun;

	private readonly float3 endPosition;

	private readonly float3 initialVelocity;

	private readonly float timeOfFlight;

	private float fallingTime;

	private bool jumpTrigger;

	private bool landTrigger;

	private bool complete;

	protected Jump(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
		: base(entity, segment, lookRotationOverride)
	{
		if (!NavMesh.SamplePosition(entity.FootPosition, out var hit, 1f, -1))
		{
			canRun = false;
			return;
		}
		if (!NavMesh.SamplePosition(segment.EndPosition, out hit, 1f, -1))
		{
			canRun = false;
			return;
		}
		endPosition = hit.position;
		float launchAngleDegrees = math.clamp(BurstPathfindingUtilities.EstimateLaunchAngle((float3)entity.FootPosition, in endPosition), 5f, 85f);
		if (!BurstPathfindingUtilities.CanReachJumpPosition((float3)entity.FootPosition, in endPosition, NpcMovementValues.MaxVerticalVelocity * 1.2f, NpcMovementValues.MaxHorizontalVelocity * 1.2f, NpcMovementValues.Gravity))
		{
			canRun = false;
		}
		else if (!BurstPathfindingUtilities.CalculateJumpVelocityWithAngle((float3)entity.FootPosition, in endPosition, launchAngleDegrees, NpcMovementValues.Gravity, out initialVelocity, out timeOfFlight) || float.IsNaN(timeOfFlight))
		{
			canRun = false;
		}
		else
		{
			canRun = true;
		}
	}

	public override bool CanRun()
	{
		return canRun;
	}

	public override bool IsComplete()
	{
		return complete;
	}

	public override bool HasFailed()
	{
		return false;
	}

	public override void WriteState(ref NpcState state)
	{
		if (jumpTrigger)
		{
			state.jumped = true;
			jumpTrigger = false;
		}
		if (landTrigger)
		{
			state.landed = true;
			landTrigger = false;
		}
		state.fallTime = fallingTime;
		state.isAirborne = true;
	}

	public override IEnumerator Execute()
	{
		float time = 0f;
		entity.Components.GoapController.LockPlan = true;
		entity.Components.Agent.updatePosition = false;
		entity.Components.Animator.SetTrigger(NpcAnimator.Jump);
		jumpTrigger = true;
		entity.Components.Animator.SetBool(NpcAnimator.Grounded, value: false);
		float3 startPosition = entity.Position;
		Vector3 forward = segment.EndPosition.ToVector3() - entity.transform.position;
		forward = new Vector3(forward.x, 0f, forward.z);
		while (time < timeOfFlight)
		{
			entity.transform.rotation = Quaternion.RotateTowards(entity.transform.rotation, Quaternion.LookRotation(forward, Vector3.up), 180f * Time.deltaTime);
			Vector3 lastPosition = entity.Position;
			Vector3 position = BurstPathfindingUtilities.GetPointOnCurve(startPosition, initialVelocity, time, NpcMovementValues.Gravity);
			entity.Position = position;
			yield return null;
			time += Time.deltaTime;
			if (lastPosition.y > position.y)
			{
				fallingTime += Time.deltaTime;
			}
		}
		if (!Physics.Raycast(entity.Position, Vector3.down, 0.6f, LayerMask.GetMask("Default")))
		{
			entity.Components.PhysicsTaker.TakePhysicsForce(0f, Vector3.down, NetClock.CurrentFrame, entity.NetworkObjectId, forceFreeFall: true);
			entity.Components.GoapController.LockPlan = false;
			complete = true;
			yield break;
		}
		if (NavMesh.SamplePosition(endPosition.ToVector3() + Vector3.up * 0.5f, out var hit, 0.5f, -1))
		{
			entity.Components.Agent.Warp(hit.position);
			entity.Components.transform.position = hit.position + Vector3.up * 0.5f;
		}
		else
		{
			entity.Components.Agent.Warp(endPosition.ToVector3() + Vector3.up * 0.5f);
			entity.transform.position = endPosition.ToVector3() + Vector3.up * 0.5f;
		}
		entity.Components.Animator.SetBool(NpcAnimator.Grounded, value: true);
		entity.Components.Animator.SetTrigger(NpcAnimator.Landed);
		landTrigger = true;
		entity.Components.Agent.updatePosition = true;
		yield return new WaitForSeconds(0.1f);
		entity.Components.GoapController.LockPlan = false;
		complete = true;
	}

	public override void Stop()
	{
		entity.Components.GoapController.LockPlan = false;
	}
}
