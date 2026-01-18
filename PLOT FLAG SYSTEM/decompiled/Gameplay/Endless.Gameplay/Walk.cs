using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class Walk : Motion
{
	private readonly NavMeshPath path = new NavMeshPath();

	private bool failedToNavigate;

	protected Walk(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
		: base(entity, segment, lookRotationOverride)
	{
	}

	public override bool CanRun()
	{
		return NavMesh.CalculatePath(entity.FootPosition, segment.EndPosition, entity.Components.Agent.areaMask, path);
	}

	public override bool IsComplete()
	{
		if (!entity.Components.Agent.pathPending && entity.Components.Agent.isOnNavMesh)
		{
			return entity.Components.Agent.remainingDistance <= entity.Components.Agent.stoppingDistance;
		}
		return false;
	}

	public override bool HasFailed()
	{
		return failedToNavigate;
	}

	public override void WriteState(ref NpcState state)
	{
	}

	public override IEnumerator Execute()
	{
		entity.Components.Agent.updateRotation = !lookRotationOverride;
		if (!entity.Components.Agent.SetPath(path) && !entity.Components.Agent.SetDestination(segment.EndPosition))
		{
			failedToNavigate = true;
		}
		else
		{
			if (!lookRotationOverride)
			{
				yield break;
			}
			while ((bool)lookRotationOverride)
			{
				Vector3 vector = lookRotationOverride.position - entity.transform.position;
				vector = new Vector3(vector.x, 0f, vector.z);
				Quaternion quaternion = Quaternion.LookRotation(vector);
				if (Quaternion.Angle(entity.transform.rotation, quaternion) > 5f)
				{
					entity.transform.rotation = Quaternion.RotateTowards(entity.transform.rotation, quaternion, Time.deltaTime * entity.Settings.RotationSpeed);
				}
				yield return null;
			}
		}
	}

	public override void Stop()
	{
		entity.Components.Agent.updateRotation = false;
		if (entity.Components.Agent.isOnNavMesh)
		{
			entity.Components.Agent.ResetPath();
		}
	}
}
