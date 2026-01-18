using System;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class BackstepStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public BackstepStrategy(NpcEntity entity)
	{
		this.entity = entity;
	}

	public void Start()
	{
		Status = GoapAction.Status.InProgress;
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: true);
		Vector3 navPosition = entity.Target.NavPosition;
		if (NavMesh.SamplePosition(navPosition + (entity.FootPosition - navPosition).normalized * 1.25f, out var hit, 0.5f, -1))
		{
			entity.Components.Pathing.RequestPath(hit.position, PathfindingResponseHandler);
		}
		else
		{
			Status = GoapAction.Status.Failed;
		}
	}

	public void Tick(uint frame)
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
		}
	}

	public void Stop()
	{
		entity.Components.Animator.SetBool(NpcAnimator.ZLock, value: false);
		entity.Components.PathFollower.StopPath();
		entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
		entity.NpcBlackboard.Set(NpcBlackboard.Key.BoredomTime, 0f);
	}

	private void PathfindingResponseHandler(Pathfinding.Response obj)
	{
		if (obj.PathfindingResult == NpcEnum.PathfindingResult.Failure)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		entity.Components.PathFollower.SetPath(obj.Path);
		entity.Components.PathFollower.LookRotationOverride = entity.Target.transform;
		entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
		entity.Components.PathFollower.OnPathFinished += HandleOnPathFinished;
	}

	private void HandleOnPathFinished(bool complete)
	{
		Status = (complete ? GoapAction.Status.Complete : GoapAction.Status.Failed);
	}
}
