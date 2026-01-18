using System;
using UnityEngine;

namespace Endless.Gameplay;

public class ApproachTargetStrategy : IActionStrategy
{
	private readonly NpcEntity entity;

	private Vector3 targetPositionAtRequest;

	public Func<float> GetCost { get; }

	public GoapAction.Status Status { get; private set; }

	public ApproachTargetStrategy(NpcEntity entity)
	{
		this.entity = entity;
	}

	public void Start()
	{
		if (!entity.Target || !entity.Target.CombatPositionGenerator.TryGetClosestAroundPosition(entity.Position, out var aroundPosition))
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		Status = GoapAction.Status.InProgress;
		targetPositionAtRequest = entity.Target.Position;
		entity.Components.Pathing.RequestPath(aroundPosition, PathfindingResponseHandler);
	}

	public void Tick(uint frame)
	{
		if ((bool)entity.Target && Vector3.Distance(targetPositionAtRequest, entity.Target.Position) > 1f)
		{
			Status = GoapAction.Status.Failed;
		}
	}

	public void Stop()
	{
		entity.Components.PathFollower.StopPath();
		entity.Components.PathFollower.OnPathFinished -= HandlePathFinished;
		entity.NpcBlackboard.Set(NpcBlackboard.Key.BoredomTime, 0f);
	}

	private void PathfindingResponseHandler(Pathfinding.Response response)
	{
		if (response.PathfindingResult == NpcEnum.PathfindingResult.Failure)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		if (!entity.Target)
		{
			Status = GoapAction.Status.Failed;
			return;
		}
		entity.Components.PathFollower.SetPath(response.Path);
		entity.Components.PathFollower.OnPathFinished -= HandlePathFinished;
		entity.Components.PathFollower.OnPathFinished += HandlePathFinished;
	}

	private void HandlePathFinished(bool completed)
	{
		Status = (completed ? GoapAction.Status.Complete : GoapAction.Status.Failed);
	}
}
