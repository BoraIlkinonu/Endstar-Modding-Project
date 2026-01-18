using System;
using UnityEngine;

namespace Endless.Gameplay;

public class MoveNearTargetStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public MoveNearTargetStrategy(NpcEntity entity)
	{
		this.entity = entity;
	}

	public void Start()
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		Status = GoapAction.Status.InProgress;
		SetDestinationToTarget();
	}

	public void Tick(uint frame)
	{
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
		}
		else if (Vector3.Distance(entity.FootPosition, entity.Components.TargeterComponent.Target.NavPosition) < entity.NearDistance)
		{
			Status = GoapAction.Status.Complete;
		}
		else if (entity.Components.PathFollower.Path != null && Vector3.Distance(entity.Components.PathFollower.Path.Destination, entity.Target.NavPosition) > 0.5f)
		{
			SetDestinationToTarget();
		}
	}

	public void Stop()
	{
		entity.Components.PathFollower.StopPath();
		entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
	}

	private void SetDestinationToTarget()
	{
		if (!entity.Target.PositionPrediction.TryGetPredictedNavigationPosition(entity.Settings.PredictionTime, out var position))
		{
			Status = GoapAction.Status.Failed;
		}
		else
		{
			entity.Components.Pathing.RequestPath(position, PathfindingResponseHandler);
		}
	}

	private void PathfindingResponseHandler(Pathfinding.Response response)
	{
		if (response.PathfindingResult == NpcEnum.PathfindingResult.Failure)
		{
			Status = GoapAction.Status.Failed;
		}
		else if ((bool)entity.Target)
		{
			entity.Components.PathFollower.SetPath(response.Path);
			if (response.Path.GetLength() < 10f)
			{
				entity.Components.PathFollower.LookRotationOverride = entity.Target.transform;
			}
			entity.Components.PathFollower.OnPathFinished -= HandleOnPathFinished;
			entity.Components.PathFollower.OnPathFinished += HandleOnPathFinished;
		}
	}

	private void HandleOnPathFinished(bool result)
	{
		Status = (result ? GoapAction.Status.Complete : GoapAction.Status.Failed);
	}
}
