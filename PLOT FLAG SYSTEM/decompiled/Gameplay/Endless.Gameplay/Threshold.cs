using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class Threshold : Motion
{
	public readonly Door Door;

	private readonly bool canRun;

	private bool thresholdPassed;

	private bool navigationUpdated;

	private bool failedToWalkThrough;

	protected Threshold(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
		: base(entity, segment, lookRotationOverride)
	{
		Door = MonoBehaviourSingleton<Pathfinding>.Instance.GetDoorFromThresholdSection(segment.StartSection);
		canRun = (bool)Door && Door.CurrentNpcDoorInteraction != Door.NpcDoorInteraction.NotOpenable;
	}

	public override bool CanRun()
	{
		return canRun;
	}

	public override bool IsComplete()
	{
		return thresholdPassed;
	}

	public override bool HasFailed()
	{
		return failedToWalkThrough;
	}

	public override void WriteState(ref NpcState state)
	{
	}

	public override IEnumerator Execute()
	{
		yield return new WaitForSeconds(0.25f);
		if (Door.IsOpenOrOpening)
		{
			if (entity.Components.Agent.SetDestination(segment.EndPosition))
			{
				failedToWalkThrough = true;
			}
		}
		else
		{
			entity.Components.Animator.SetTrigger(NpcAnimator.Interact);
			if (Door.IsLocked)
			{
				Lockable lockable = Door.WorldObject.GetUserComponent<Lockable>();
				foreach (NpcEntity npc in MonoBehaviourSingleton<NpcManager>.Instance.Npcs)
				{
					if (!(npc == entity) && npc.Group == entity.Group && npc.Components.TargeterComponent.KnownHittables.Contains(entity.Components.HittableComponent))
					{
						npc.Components.Pathing.ExcludeEdge(lockable, segment);
					}
				}
				yield return new WaitForSeconds(0.75f);
				entity.Components.Pathing.ExcludeEdge(lockable, segment);
				failedToWalkThrough = true;
				yield break;
			}
			Door.Open(entity.Context, forwardDirection: true);
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= HandleNavigationUpdated;
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated += HandleNavigationUpdated;
			yield return new WaitUntil(() => navigationUpdated);
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= HandleNavigationUpdated;
			if (!entity.Components.Agent.SetDestination(segment.EndPosition))
			{
				failedToWalkThrough = true;
				yield break;
			}
			if (!entity.Components.GoapController.HasCombatPlan && Door.CurrentNpcDoorInteraction == Door.NpcDoorInteraction.OpenAndCloseBehind)
			{
				while (entity.Components.Agent.pathPending || entity.Components.Agent.remainingDistance > entity.Components.Agent.stoppingDistance)
				{
					yield return null;
				}
				float3 @float = segment.StartPosition - segment.EndPosition;
				@float = new float3(@float.x, 0f, @float.z);
				Quaternion rotation = quaternion.LookRotation(@float, math.up());
				while (Quaternion.Angle(entity.transform.rotation, rotation) > 5f)
				{
					entity.transform.rotation = Quaternion.RotateTowards(entity.transform.rotation, rotation, entity.Settings.RotationSpeed * Time.deltaTime);
					yield return null;
				}
				Door.Close(entity.Context);
				entity.Components.Animator.SetTrigger(NpcAnimator.Interact);
				yield return new WaitForSeconds(0.25f);
			}
		}
		while (entity.Components.Agent.remainingDistance > 0.05f)
		{
			yield return null;
		}
		thresholdPassed = true;
	}

	private void HandleNavigationUpdated(HashSet<SerializableGuid> updatedProps)
	{
		if (updatedProps.Contains(Door.WorldObject.InstanceId))
		{
			navigationUpdated = true;
		}
	}

	public override void Stop()
	{
		if (entity.Components.Agent.isOnNavMesh)
		{
			entity.Components.Agent.ResetPath();
		}
		MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= HandleNavigationUpdated;
	}
}
